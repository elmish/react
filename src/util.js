import * as React from "react";

export function createLazyView(displayName, areEqual, render) {
    const LazyView = class extends React.Component {
        constructor(props) {
            super(props);
        }
        shouldComponentUpdate(nextProps, _nextState) {
            return !areEqual(this.props.model, nextProps.model);
        }
        render() {
            return render(this.props.state, this.props.dispatch);
        }
    };
    LazyView.displayName = displayName || "LazyView";
    return ([key, model, dispatch]) => React.createElement(LazyView, { key, model, dispatch });
}