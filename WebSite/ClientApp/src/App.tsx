import * as React from 'react';
import { Route } from 'react-router';
import Layout from './components/Layout';
import Home from './components/Home';
import Counter from './components/Counter';
import FetchData from './components/FetchData';
import Login from './components/Login';
import PrivateRoute from './components/PrivateRoute';

import './custom.css'

export default () => (
    <Layout>
        <PrivateRoute path='/counter'>
            <Counter />
        </PrivateRoute>
        <PrivateRoute path='/fetch-data/:startDateIndex?'>
            <FetchData />
        </PrivateRoute>
        <Route path='/login' component={Login} />
        <PrivateRoute index exact path='/'>
            <Home />
        </PrivateRoute>
    </Layout>
);
