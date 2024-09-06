import { Action, Reducer } from 'redux';

// -----------------
// STATE - This defines the type of data maintained in the Redux store.

export interface AuthState {
    isAtuheticated: boolean;
    username: string;
}

// -----------------
// ACTIONS - These are serializable (hence replayable) descriptions of state transitions.
// They do not themselves have any side-effects; they just describe something that is going to happen.
// Use @typeName and isActionType for type detection that works even after serialization/deserialization.

export interface LoginAction { type: 'LOGIN_USER', Username:string,Token:string }
export interface LogoutAction { type: 'LOGOUT_USER' }

// Declare a 'discriminated union' type. This guarantees that all references to 'type' properties contain one of the
// declared type strings (and not any other arbitrary string).
export type KnownAction = LoginAction | LogoutAction;

// ----------------
// ACTION CREATORS - These are functions exposed to UI components that will trigger a state transition.
// They don't directly mutate state, but they can have external side-effects (such as loading data).

export const actionCreators = {
    login: (username: string, password: string) => {
        return function (dispatch) {
            const requestOptions = {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ Username: username, Password: password })
            };
            console.log('fetch');
            return fetch('/auth/authenticate', requestOptions)
                .then(response => {
                    response.text().then(text => {
                        const data = text && JSON.parse(text);
                        if (!response.ok) {
                            if (response.status === 401) {
                                // auto logout if 401 response returned from api
                                return { type: 'LOGOUT_USER' }
                            }

                            const error = (data && data.message) || response.statusText;
                            return Promise.reject(error);
                        }
                        console.log('OK!');
                        dispatch({ type: 'LOGIN_USER', Username: data.Username, Token: data.Token });
                    })
                });
        }
    },
    logout: () => ({ type: 'LOGOUT_USER' } as LogoutAction)
};

// ----------------
// REDUCER - For a given state and action, returns the new state. To support time travel, this must not mutate the old state.

export const reducer: Reducer<AuthState> = (state: AuthState | undefined, incomingAction: Action): AuthState => {
    if (state === undefined) {
        return { isAtuheticated: false, username: '' };
    }

    const action = incomingAction as KnownAction;
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
