# Virtual Suspect Alexa Skill

This is the repository for all the code that was used during the development of my Master's Thesis ("The Virtual Suspect Meets Alexa: Designing a Brand New Natural Language Interaction").
The focus of this work was to design a Natural Language Interaction with the Virtual Suspect, using an Alexa Skill to achieve that goal.
If you're curious, you can read the Extended Abstract [here](tree/master/docs/Resumo_Alargado_Goncalo_Baptista.pdf), or the full Dissertation [here](tree/master/docs/Dissertacao_Goncalo_Baptista.pdf).

In terms of code, this work is composed of 4 essential parts:

 - The [Interaction Model](../model)
 
     An essential part of every Alexa Skill, the Interaction Model parses the users' questions in a way that the Virtual Suspect's query system can process.
	 
 - The [Skill Service](../VirtualSuspectLamda)
 
     A Lambda function that takes the information from the Interaction Model and transforms it into Virtual Suspect queries, as well as all the error verifications and a lot of the internal logic.
	 
 - The [Virtual Suspect Response Model](../VirtualSuspect)
 
     One of the two components inherited from the original Virtual Suspect prototype, this module contains all the Virtual Suspect logic, the Knowledge Base, the Query Engine, the lying components, and everything that makes it tick.	
	
 - The [Natural Language Generator](../VirtualSuspectNaturalLanguage)
 
     The other component inherited from the original prototype, this module transforms the responses from the Virtual Suspect into actual sentences.
	 
## How to try it

You can add this Skill to your Alexa by searching for "Virtual Suspect Demo" in your Alexa Store.

Before you start interacting with the Virtual Suspect, I recommend that you read [this document](https://docs.google.com/document/d/1fLmwlODrWwCj-fKYrzdj9xXMwXVt9U2qY7ZSw8JDmxM/edit?usp=sharing), containing all the information and context required to interrogate the Suspect.

### Limitations

Disclaimer: this is just a prototype developed as part of a dissertation, and not a fully-fledged voice game. It has some limitations and shortcomings that may sometimes lead to an imperfect experience. Despite this, we were still able to obtain good User Experience results, and it's still possible to have decent conversations with the Suspect.
Some of these limitations include:
 - An unintuitive lying algorithm (which was outside the scope of my work)
 - An inability to understand questions outside of the conversation domain

Thank you for your patience!

If you have any questions you can contact me at: gms_baptista@sapo.pt