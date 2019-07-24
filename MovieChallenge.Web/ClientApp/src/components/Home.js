import React, { Component, Fragment } from "react";
import SearchBar from "./SearchBar";

export class Home extends Component {
    static displayName = Home.name;

    render() {
        return (
            <Fragment>
                <SearchBar />
            </Fragment>
        );
    }
}
