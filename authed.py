import requests
import jwt

class Authed:
    baseUri = "https://api.authed.io/app"

    def __init__(self, _id, access, userKey):
        self.appId = _id
        self.access = access
        self.userKey = userKey
        self.userSession = None
        self.appSession = None

    def _call_api(self, endpoint, body, _headers):
        req = requests.post(endpoint, data=body, headers=_headers)
        return req.json()

    def verify(self):
        json = self._call_api(baseUri + "/verify/" + self.appId, self.access, None)
        if json['session'] != None:
            self.appSession = json['session']
            return True
        else:
            raise Exception('{}: {}'.format(json['status'], json['message']))

    def login(self, email, password):
        headers = { 'session': self.appSession }
        body = {
            'email': email,
            'password': password,
        }
        json = self._call_api(baseUri + "/login", body, headers)

        if json['status'] == 200:
            self.userSession = json['userSesion']
            return True
        else:
            raise Exception('{}: {}'.format(json['status'], json['message']))

    
    def register(self, email, password, _license):
        headers = { 'session': self.appSession }
        body = {
            'email': email,
            'password': password,
            'licenseCode': _license
        }

        json = self._call_api(baseUri + "/register", body, headers)

        if json['status'] == 200:
            return True
        else:
            raise Exception('{}: {}'.format(json['status'], json['message']))

    def renew(self, email, _license):
        headers = { 'session': self.appSession }
        body = {
            'email': email,
            'licenseCode': _license
        }

        json = self._call_api(baseUri + "/renew", body, headers)

        if json['stats'] == 200:
            return True
        else: 
            raise Exception('{}: {}'.format(json['status'], json['message']))

    def generate_license(self, secret, prefix, amount, level, time):
        headers = { 'session': self.appSession }
        body = {
            'secret': secret,
            'prefix': prefix,
            'amount': amount,
            'level': level,
            'time': time,
        }

        json = self._call_api(self, body, headers)

        if json['status'] == 200:
            return json['message']
        else:
            raise Exception('{}: {}'.format(json['status'], json['message']))

    def update_user(self, field, value):
        headers = { 'session': self.appSession }
        body = {
            'userSession': self.userSession,
            'field': field,
            'value': value
        }

        json = self._call_api(baseUri + "/users/set", body, headers)

        if json['status'] == 200:
            return True
        else:
            raise Exception('{}: {}'.format(json['status'], json['message']))

    def update_app(self, field, value, secret):
        headers = { 'session': self.appSession }
        body = {
            'secret': secret,
            'value': value
        }

        json = self._call_api(baseUri + "/set/" + field, body, headers)

        if json['status'] == 200:
            return True
        else:
            raise Exception('{}: {}'.format(json['status'], json['message']))

    # worning this is only going to work if you set a custom user key on Authed
    def verifyUserSession(self):
        return jwt.decode(self.userSession, self.userKey, algorithms=['HS256'])