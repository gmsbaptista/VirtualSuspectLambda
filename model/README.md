# Interaction Model

The Interaction Model for the Virtual Suspect Skill was structured around the types of queries that the Virtual Suspect can understand.

Initially, there were only the intents for the basic types of questions: yes/no, when, where, who, why, what; but as more functionalities were added, new intents were defined for those purposes.
I opted for a design in which as many words as possible were covered by slots so that as many different iterations of sentences with the same meaning could be covered with less authoring effort.

Two of the biggest challenges when designing this Interaction Model were the way that all the different time slots supported by the default Amazon slot types could interact with the time conditions of the Virtual Suspect queries (I even created a script to try and make sure all the possibilities were covered, as can be seen [here](/model/scripts)), and all the small variations and possibilities of words that could have the same meaning, like optional prepositions in front of more relevant slots.

At some point, I considered exploring two different structures for the Interaction Model, but then quickly abandoned the idea, and those old and new files are just a relic of that.