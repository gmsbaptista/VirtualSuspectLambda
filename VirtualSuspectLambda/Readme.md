# Virtual Suspect Skill Service

This module contains the Lambda function that functions as the Virtual Suspect Skill Service, and the file containing all the details of the agent's story that populates its Knowledge Base.

## Lambda Function

The main purpose of this function is to decode the information sent by the Interaction Model into a format the Virtual Suspect can process.
This mostly consists of creating a Query object, adding a relevant Focus, and then adding all the relevant query Conditions, according to the slots present in the Intent.
This seems simple enough, but as more functionalities were added to the Virtual Suspect Skill throughout development, I added more checks, verifications, and secondary functions, to ensure the smoothness of the interaction.
This includes a record of the context of the conversation, pronoun capability, verifications for the number of conditions and the validity of the slots, etc.

## Story

There are two story files, one that was inherited from the original prototype and went through several changes throughout the development process, and a second one that I wrote that maintained the same general structure of the original story but changed certain events and entities.
Both story files define all the entities present in the story in the beginning, followed by all the events of the story, which include said entities.