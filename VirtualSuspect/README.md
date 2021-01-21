# Virtual Suspect Response Model

This module, and most of its code, was inherited from the original Virtual Suspect work and prototype, although I did make a lot of modifications as more functionalities were added to the Skill.

It contains all the logic for the Virtual Suspect Response Model:
- the **Knowledge Base**, how it is structured and how all the entities, actions, and events are defined;
- the **Query Engine**, how the queries are defined and structured, all the predicates that make it work, and the logic for the question/answer dynamic;
- the **Lying components**, namely the Handlers that envelop the query process, although this was the component that received the least amount of changes, as it did not pertain to the focus of my work.

## Changes

I added a lot of functionality to the original Virtual Suspect prototype, including support for new types of questions that were not originally supported, by adding lots of new predicates and their logic, and a restructuring of the Knowledge Base to treat Actions as pseudo-entities.

## Shortcomings

The way the lying algorithm is setup definitely leaves something to be desired, especially when combined with all the other improvements to the Virtual Suspect and the interaction, and I wish I had had the opportunity to revamp it, but it was outside the scope of the work and other time constraints didn't make it possible.