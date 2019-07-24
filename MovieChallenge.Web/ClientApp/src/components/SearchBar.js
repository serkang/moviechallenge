import React, { Component, Fragment } from "react";
import axios from "axios";
import SearchResults from './SearchResults';

class SearchBar extends Component {
    constructor(props) {
        super(props);
        this.state = {
            timeout: null,
            results: []
        };
    }

    Search = e => {
        var keyword = e.target.value;
        clearTimeout(this.state.timeout);
        if (keyword.length >= 3) {
            this.setState({
                timeout: setTimeout(() => {
                    console.log(keyword);
                    axios.get(`https://localhost:44364/api/search?keyword=${keyword}`)
                        .then(res => this.setState({
                            results : res.data
                        }));
                }, 500)
            });
        }
    };

    render() {
        const {results} = this.state;
        return (
            <Fragment>
                <div className="row">
                    <div className="col-md-12 text-center mt-5">
                        <h4>Search movies, series or episodes</h4>
                    </div>
                </div>
                <div className="row">
                    <div className="col-md-4 offset-md-4 mt-5">
                        <input
                            type="text"
                            className="form-control"
                            placeholder="Min. 3 chars"
                            onChange={e => {
                                this.Search(e);
                            }}
                        />
                    </div>
                </div>
                <div className="row">
                    <div className="col-md-12 mt-5 text-center">
                        <SearchResults result = {results} />
                    </div>
                </div>
            </Fragment>
        );
    }
}

export default SearchBar;
