import React, { Component } from 'react';
import { Route } from 'react-router';
import { Layout } from './components/Layout';
import { Home } from './components/Home';
import Detail from './components/Detail';
import {BrowserRouter as Router, Redirect, Switch} from 'react-router-dom';

export default class App extends Component {
  static displayName = App.name;

  render () {
    return (
      <Layout>
            <Router>
              <Switch>
                <Route exact path='/' component={Home} />
                <Route exact path='/detail/:id' component={Detail} />
                <Route render={()=><Redirect to="/"/>} />
              </Switch>
            </Router>
      </Layout>
    );
  }
}
