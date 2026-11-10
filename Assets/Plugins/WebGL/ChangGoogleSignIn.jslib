// Chang: Google sign-in for WebGL (GoogleWebSignIn.cs). Google's OpenID Connect page opens in a popup with
// response_type=id_token; Google returns to google-auth/ of this site, that page sends the ID token back here
// (BroadcastChannel, or window.opener when the popup still has it) and closes. See Docs/save-and-auth.md#google.
//
// Why a popup and not google.accounts.id (Google Identity Services):
// - google.accounts.id.prompt() (One Tap / FedCM) is not shown again for hours after the player closes it once,
//   so it can't back a button;
// - the GIS button (renderButton) is an iframe that must be clicked by the player: it would have to float as HTML
//   over the Unity canvas and follow the Unity layout.
// The popup gives the same Google ID token and works from any Unity button.
var ChangGoogleSignInLib = {
  $ChangGoogleSignIn: {
    channelName: 'chang-google-signin',
    pending: null,

    randomString: function () {
      var bytes = new Uint8Array(16);
      crypto.getRandomValues(bytes);
      return Array.prototype.map.call(bytes, function (b) { return ('0' + b.toString(16)).slice(-2); }).join('');
    },

    redirectUri: function () {
      // the folder of the game page, e.g. https://kruchaang.win/google-auth/
      return location.origin + location.pathname.replace(/[^\/]*$/, '') + 'google-auth/';
    },
  },

  ChangGoogleSignIn_Start__deps: ['$ChangGoogleSignIn'],
  ChangGoogleSignIn_Start: function (clientIdPtr, callback) {
    // the values are the same in GoogleWebSignIn.cs
    var STATUS_SUCCESS = 0, STATUS_CANCELED = 1, STATUS_ERROR = 2;
    var lib = ChangGoogleSignIn;
    var clientId = UTF8ToString(clientIdPtr);

    if (lib.pending) {
      lib.pending.finish(STATUS_CANCELED, '');
    }

    var state = lib.randomString();
    var pending = { done: false };
    var channel = null;
    var cancelTimer = 0;
    var timeoutTimer = 0;

    function cleanup() {
      window.removeEventListener('message', onWindowMessage);
      window.removeEventListener('focus', onFocus);
      document.removeEventListener('visibilitychange', onVisibilityChange);
      clearTimeout(cancelTimer);
      clearTimeout(timeoutTimer);
      if (channel) {
        channel.close();
      }
    }

    pending.finish = function (status, value) {
      if (pending.done) {
        return;
      }

      pending.done = true;
      if (lib.pending === pending) {
        lib.pending = null;
      }

      cleanup();
      var buffer = stringToNewUTF8(value || '');
      {{{ makeDynCall('vii', 'callback') }}}(status, buffer);
      _free(buffer);
    };

    function onResult(data) {
      if (!data || data.source !== lib.channelName || data.state !== state) {
        return;
      }

      if (data.idToken) {
        pending.finish(STATUS_SUCCESS, data.idToken);
      } else if (data.error === 'access_denied' || data.error === 'user_cancelled') {
        pending.finish(STATUS_CANCELED, '');
      } else {
        pending.finish(STATUS_ERROR, data.error || 'no id_token in the answer');
      }
    }

    function onWindowMessage(e) {
      if (e.origin === location.origin) {
        onResult(e.data);
      }
    }

    // The player closed the popup: the game gets the focus back and no answer comes. popup.closed can't be used:
    // the Google page cuts the popup from this window (Cross-Origin-Opener-Policy), then it always reads true.
    function onFocus() {
      clearTimeout(cancelTimer);
      cancelTimer = setTimeout(function () { pending.finish(STATUS_CANCELED, ''); }, 1500);
    }

    function onVisibilityChange() {
      if (document.visibilityState === 'visible') {
        onFocus();
      }
    }

    var params = new URLSearchParams({
      client_id: clientId,
      redirect_uri: lib.redirectUri(),
      response_type: 'id_token',
      scope: 'openid email profile',
      nonce: lib.randomString(),
      state: state,
      prompt: 'select_account',
    });

    var width = 500, height = 640;
    var left = Math.max(0, (window.screenX || 0) + ((window.outerWidth || width) - width) / 2);
    var top = Math.max(0, (window.screenY || 0) + ((window.outerHeight || height) - height) / 2);
    var popup = window.open('https://accounts.google.com/o/oauth2/v2/auth?' + params.toString(), lib.channelName,
      'popup,width=' + width + ',height=' + height + ',left=' + Math.round(left) + ',top=' + Math.round(top));

    lib.pending = pending;
    if (!popup) {
      pending.finish(STATUS_ERROR, 'popup_blocked');
      return;
    }

    if (typeof BroadcastChannel !== 'undefined') {
      channel = new BroadcastChannel(lib.channelName);
      channel.onmessage = function (e) { onResult(e.data); };
    }

    window.addEventListener('message', onWindowMessage);
    // added after the popup is open: the focus moves to it first
    setTimeout(function () {
      if (!pending.done) {
        window.addEventListener('focus', onFocus);
        document.addEventListener('visibilitychange', onVisibilityChange);
      }
    }, 500);
    timeoutTimer = setTimeout(function () { pending.finish(STATUS_CANCELED, ''); }, 10 * 60 * 1000);
  },
};

autoAddDeps(ChangGoogleSignInLib, '$ChangGoogleSignIn');
mergeInto(LibraryManager.library, ChangGoogleSignInLib);
