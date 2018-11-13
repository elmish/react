import * as React from "react";

export function createLazyView(displayName) {
    const LazyView = class extends React.Component {
        constructor(props) {
            super(props);
        }
        shouldComponentUpdate(nextProps, _nextState) {
            return !this.props.equal(this.props.model, nextProps.model);
        }
        render() {
            return this.props.render();
        }
    };
    LazyView.displayName = displayName || "LazyView";
    return LazyView;
}