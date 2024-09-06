import * as React from 'react';
import { connect } from 'react-redux';
import { Redirect, Route } from 'react-router';
import { ApplicationState } from '../store';
import * as AuthStore from '../store/Auth';



class PrivateRoute extends React.PureComponent<any> {
    public render() {
        var { children, ...routeParams } = this.props.ownProps;
        console.log(this.props.path);
        return (
            <Route {...routeParams}

                render={() => {
                    return this.props.auth.isAtuheticated === true ? (
                        this.props.ownProps.children
                    ) : (
                        <Redirect to="/login" />
                    );
                }}
            />
        );
    }
};

export default connect(
    (state: ApplicationState, ownProps: any): any => ({ auth: state.auth, ownProps: ownProps }),
    AuthStore.actionCreators
)(PrivateRoute);
