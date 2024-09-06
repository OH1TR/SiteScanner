import * as React from 'react';
import { connect } from 'react-redux';
import { Collapse, Container, Navbar, NavbarBrand, NavbarToggler, NavItem, NavLink } from 'reactstrap';
import { RouteComponentProps } from 'react-router';
import { ApplicationState } from '../store';
import { Link } from 'react-router-dom';
import './NavMenu.css';
import * as AuthStore from '../store/Auth';

type NavMenuProps =
    AuthStore.AuthState &
    typeof AuthStore.actionCreators &
    RouteComponentProps<{}>;

class NavMenu extends React.PureComponent<NavMenuProps> {

    public render() {
        return (
            <header>
                <Navbar className="navbar-expand-sm navbar-toggleable-sm border-bottom box-shadow mb-3" light>
                    <Container>
                        <NavbarBrand tag={Link} to="/">Site Scanner</NavbarBrand>
                        <NavbarToggler className="mr-2"/>
                        <Collapse className="d-sm-inline-flex flex-sm-row-reverse" isOpen={true} navbar>
                            {this.props.isAtuheticated===true &&
                            (
                                <ul className="navbar-nav flex-grow">
                                    <NavItem>
                                        <NavLink tag={Link} className="text-dark" to="/">Home</NavLink>
                                    </NavItem>
                                    <NavItem>
                                        <NavLink tag={Link} className="text-dark" to="/counter">Counter</NavLink>
                                    </NavItem>
                                    <NavItem>
                                        <NavLink tag={Link} className="text-dark" to="/fetch-data">Fetch data</NavLink>
                                    </NavItem>
                                    <button onClick={() => { this.props.logout(); }}>Logout</button>
                                </ul>
                            )
                        }                            
                        </Collapse>
                    </Container>
                </Navbar>
            </header>
        );
    }
};

    export default connect(
    (state: ApplicationState) => state.auth,
    AuthStore.actionCreators
)(NavMenu);
