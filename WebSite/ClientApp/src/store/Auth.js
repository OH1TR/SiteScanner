"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.reducer = exports.actionCreators = void 0;
// ----------------
// ACTION CREATORS - These are functions exposed to UI components that will trigger a state transition.
// They don't directly mutate state, but they can have external side-effects (such as loading data).
exports.actionCreators = {
    login: function (username, password) {
        return function (dispatch) {
            var requestOptions = {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ Username: username, Password: password })
            };
            console.log('fetch');
            return fetch('/auth/authenticate', requestOptions)
                .then(function (response) {
                response.text().then(function (text) {
                    var data = text && JSON.parse(text);
                    if (!response.ok) {
                        if (response.status === 401) {
                            // auto logout if 401 response returned from api
                            return { type: 'LOGOUT_USER' };
                        }
                        var error = (data && data.message) || response.statusText;
                        return Promise.reject(error);
                    }
                    console.log('OK!');
                    dispatch({ type: 'LOGIN_USER', Username: data.Username, Token: data.Token });
                });
            });
        };
    },
    logout: function () { return ({ type: 'LOGOUT_USER' }); }
};
// ----------------
// REDUCER - For a given state and action, returns the new state. To support time travel, this must not mutate the old state.
var reducer = function (state, incomingAction) {
    if (state === undefined) {
        return { isAtuheticated: false, username: '' };
    }
    var action = incomingAction;
    switch (action.type) {
        case 'LOGIN_USER':
            console.log('Login!!');
            return { isAtuheticated: true, username: action.Username };
        case 'LOGOUT_USER':
            return { isAtuheticated: false, username: '' };
        default:
            return state;
    }
};
exports.reducer = reducer;
//# sourceMappingURL=Auth.js.map