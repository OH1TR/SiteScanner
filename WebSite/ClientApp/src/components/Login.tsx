import * as React from 'react';
import { connect } from 'react-redux';
import { RouteComponentProps } from 'react-router';
import { ApplicationState } from '../store';
import * as AuthStore from '../store/Auth';
import { Redirect } from 'react-router';

type LoginProps =
    AuthStore.AuthState &
    typeof AuthStore.actionCreators &
    RouteComponentProps<{}>;

type LoginState = {
    username?: string;
    password?: string;
    submitted?: boolean;
};

class Login extends React.PureComponent<LoginProps, LoginState> {
    constructor(props: LoginProps) {
        super(props);

        this.state = {
            username: '',
            password: '',
            submitted: false
        };

        this.handleChange = this.handleChange.bind(this);
        this.handleSubmit = this.handleSubmit.bind(this);
    }
    handleChange(e: any) {
        const { name, value } = e.target;
        this.setState({ [name]: value });
    }

    handleSubmit(e: React.FormEvent<HTMLFormElement>) {
        e.preventDefault();
        this.setState({ submitted: true });
        const { username, password } = this.state;
        if (username && password) {
            this.props.login(username, password);
        }
    }

    public render() {
        if (this.props.isAtuheticated === true) {
            return (<Redirect to="/" />);
        }
        else {
            return (
                <React.Fragment>
                    <div className="row d-flex justify-content-center">
                        <div className="col-md-6">
                            <form className="card px-5 py-5" onSubmit={this.handleSubmit}>
                                <h1>Login</h1>
                                <div className="form-group">
                                    <label >Username</label>
                                    <input type="text" className="form-control" name="username" value={this.state.username} onChange={this.handleChange} aria-describedby="emailHelp" placeholder="Enter username" />
                                </div>
                                <div className="form-group">
                                    <label>Password</label>
                                    <input type="password" className="form-control" value={this.state.password} onChange={this.handleChange} name="password" placeholder="Password" />

                                </div>
                                <button type="submit" className="btn btn-primary btn-lg">
                                    Login
                                </button>
                            </form>
                        </div>
                    </div>
                </React.Fragment>
            );
        }
    }
}

export default connect(
    (state: ApplicationState) => state.auth,
    AuthStore.actionCreators
)(Login);
