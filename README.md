# Modifiers

What is a Behaviour?
A behaviour is a pure capability. It knows HOW to do exactly one thing. It has stats that affect that thing. It does NOT know:

- When it executes
- What triggers it
- What happens after it executes
- Anything about events

What is a Skill?
A skill is a container of behaviours. Nothing more. It doesn't orchestrate. It doesn't have triggers. It's just a bag of capabilities with combined tags.

Who orchestrates?
The GAME orchestrates. Not the library. The library cannot possibly know all the trigger types your game will have.