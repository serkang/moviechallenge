import React, { Component, Fragment } from "react";

class SearchResults extends Component {
    constructor(props) {
        super(props);
    }
    render() {
        const { result } = this.props;
        if(result && result.Search && result.Search.length > 0){
            return (
                <Fragment>
                    <div className="row">
                        <div className="col-md-12">Found Total: {result.totalResults}</div>
                    </div>
                    <div className="row">
                        {result.Search.map((value, index) => {
                            var imgsrc = value.Poster === "N/A" ? "NoImage.png" : value.Poster;
                            return (
                                <div key={index} className="col-md-4">
                                    <div className="card m-2" style={{ width: 18 + 'rem' }}>
                                        <img className="card-img-top" src={imgsrc} alt={value.Title} />
                                        <div className="card-body">
                                            <h5 className="card-title">{value.Title}</h5>
                                            <p className="card-text">Year: {value.Year}</p>
                                            <a href={"detail/" + value.imdbID} className="btn btn-primary">Details</a>
                                        </div>
                                    </div>
                                </div>
                            )
                        })}
                    </div>
                </Fragment>
            );
        } else if (result && result.Error) {
            return (
                <Fragment>
                    {result.Error}
                </Fragment>
                )
        }
        else {
            return(".")
        }
    }
}
 

export default SearchResults;