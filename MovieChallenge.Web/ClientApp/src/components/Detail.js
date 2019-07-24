import React, { Component, Fragment } from "react";
import { Row, Col, ListGroup, ListGroupItem, ListGroupItemHeading, ListGroupItemText } from 'reactstrap';
import axios from "axios";

class Detail extends Component {
    constructor(props) {
        super(props);
        this.state = { movieId : "", movieDetails : {} }
    }

    componentDidMount() {
        var mId = this.props.match.params.id;
        this.setState({
            movieId: mId
        });
        axios.get(`https://localhost:44364/api/detail?id=${mId}`)
            .then(res => {
                console.log(res.data);
                this.setState({
                    movieDetails: res.data
                });
            });
    }

    render() {
        const { movieDetails } = this.state;
        if (movieDetails && movieDetails.Error) {
            return (
                <Row>
                    <Col md="12" className="text-center">{movieDetails.Error}<br /><a href="/" className="btn btn-primary"> &lt;-Back </a></Col>
                </Row>
                );
        } else {
            var imgsrc = movieDetails.Poster === "N/A" ? "NoImage.png" : movieDetails.Poster;
            return (
                <Fragment>
                    <Row>
                        <Col md="12" className="text-center"><h3>{movieDetails.Title}</h3></Col>
                    </Row>
                    <Row>
                        <Col md={{ size: 6, offset: 3 }} className="text-center">
                            <img src={imgsrc} alt={movieDetails.Title} />
                            <ListGroup className="m-3">
                                {Object.keys(movieDetails).map((v, i) => {
                                    var bl = ["Title", "Poster", "Response", "imdbID", "Ratings"]
                                    if (!bl.includes(v)) {
                                        var t = movieDetails[v].toString();
                                        return (
                                            <ListGroupItem key={i}>
                                                <ListGroupItemHeading>{v}</ListGroupItemHeading>
                                                <ListGroupItemText>
                                                    {t}
                                                </ListGroupItemText>
                                            </ListGroupItem>
                                        )
                                    } else if (v === "Ratings") {
                                        return (
                                            <ListGroupItem key={i}>
                                                <ListGroupItemHeading>{v}</ListGroupItemHeading>
                                                <ListGroupItemText>
                                                    {movieDetails.Ratings.map((value, index) => {
                                                        return (
                                                            <span key={index}>
                                                                {value.Source}: <b>{value.Value}</b><br/>
                                                            </span>
                                                        )
                                                    })}
                                                </ListGroupItemText>
                                            </ListGroupItem>
                                            )
                                    }
                            })}
                            </ListGroup>
                            <a href="/" className="btn btn-primary"> &lt;-Back </a>
                        </Col>
                    </Row>
                </Fragment>

                );
        }
    }
}
 
export default Detail;