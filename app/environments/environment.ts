export const environment = {
  production: false,
  appName: 'PU Spec Sheet',
  apiUrl: 'https://localhost:8888',

  signalRUrl: 'https://localhost:8889',
  signalRHubName: 'checkinouthub',

  documentationUrl: '',

  auth: {
    clientId: '',
    authority: '',
    resourceMap: {
      'https://graph.microsoft.com': ['user.read'],
      'https://localhost:8888': [''],
    },
  },
};
