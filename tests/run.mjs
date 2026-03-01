// Initialise jsdom globals (window, document, navigator, etc.)
// BEFORE Fable-compiled code imports react / react-dom,
// which inspect the global scope at import time.
import 'global-jsdom/register';

// Now load the compiled test entry point.
await import('./out/Main.js');
