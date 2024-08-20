(function () {
    // Push a new state to the history stack
    history.pushState(null, null, location.href);

    // Handle the popstate event when the user presses the back button
    window.addEventListener('popstate', function () {
        history.go(1); // Move forward in history to prevent going back
    });

    // Optionally, handle forward navigation as well
    window.addEventListener('load', function () {
        // Reload the current page if navigated forward
        history.replaceState(null, null, location.href);
    });
})();
