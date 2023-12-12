# Furbo Alexa invoker

Uses the Porcupine library from [Picovoice](https://picovoice.ai/) to listen for 'hey furby' wake words, invoking the Alexa sample app on wake.

As the sample application uses a keystroke to wake, this invoker cheats by running the application using `tmux` and sending it keystrokes.