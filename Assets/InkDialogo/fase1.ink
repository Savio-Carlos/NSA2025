// --- VARIÁVEIS DA FASE ---
// 1. Variável para o estado geral da fase (controlada pelo C#)
VAR fase_status = "Inicio" 
// 2. Variáveis booleanas para cada descoberta (controladas pelo C#)
VAR encontrou_vegetacao = false
VAR encontrou_enchente = false

=== Fazendeiro ===

{ fase_status:
    - "Inicio": 
        -> inicio
    - "Andamento": 
        -> andamento
    - "Finalizada":
        -> final
    - else:
        -> generico // Um diálogo padrão caso algo dê errado
}

= inicio
# speaker: Expert 
Good morning, sir. I’m the Expert you called in. Are you Lauro, the rice farmer?
#speaker: Lauro
Good morning. Yes, I’m Lauro. Thank you for your visit! I’ve called you because, after what happened last year, it’s hard to feel confident. The river… it came like nothing I’ve ever seen. 
# speaker: Expert 
I can imagine. That flood was devastating. Did it destroy most of your crops?
#speaker: Lauro
It swept through everything. Walls, pumps, and rice almost ready to harvest were all gone. I’ve never seen the river move with such force. Whenever the clouds roll in, I can’t help but worry it might happen again.

# speaker: Expert 
It swept through everything. Walls, pumps, and rice almost ready to harvest were all gone. I’ve never seen the river move with such force. Whenever the clouds roll in, I can’t help but worry it might happen again.
# speaker: Expert 
Mr. Lauro, I might be able to help. With the satellite images I have here, we can analyze how the water behaved on your property and identify the vulnerable spots. If you allow me, we can look at them together.
#speaker: Lauro
Of course! Anything that can help me avoid that nightmare again, I would like to see.
-> END

= andamento
#speaker: Lauro
Then, do you have anything to say?
+ Not yet Mr.Lauro, i will come back when i find something. -> DONE
+ {encontrou_vegetacao} > Link the lack of native vegetation to the region's flood paths.
-> dialogo_vegetacao
+ {encontrou_enchente} > Talk about the flood footprint and how it advanced. -> dialogo_enchente
// Esta escolha só aparece quando TODAS as observações forem verdadeiras
* {encontrou_vegetacao and encontrou_enchente} > Connect the lack of native vegetation with the flood's destructive path. -> dialogo_final
-> END

= final
#speaker: Lauro
Obrigado pela ajuda bixo la de cima!
-> END

= generico 
#speaker: Lauro
Hmm, não tenho nada novo para te dizer agora.
-> END

=== dialogo_vegetacao ===
# speaker: Expert 
And that makes perfect sense, Mr. Lauro. Because when we analyze the region's vegetation maps, we see a clear pattern: those very "paths" the water uses to advance with more force are, most of the time, areas on the riverbank with very weak or non-existent native vegetation. 
# speaker: Expert 
In contrast to the deep green of the crops, these strips look like fragile scars on the landscape. Would you say the riverbank around here is also like that, more "clear"?
#speaker: Lauro
Look, for us it's simple: every square meter counts. The land on the bank is fertile and gets water easily. Letting wild growth take over a piece of ground as good as that? It's like throwing money away. We clear it so we can plant and secure the harvest.
-> END

=== dialogo_fogo ===
# speaker: Expert 
Eu também verifiquei os registros de focos de calor e, como esperado para a sua cultura, não há praticamente nenhuma atividade de queimada na sua propriedade em nenhum dos anos. Seu manejo é todo baseado na água e no preparo mecânico do solo, correto?
#speaker: Lauro
Isso mesmo. Fogo aqui, só na churrasqueira. Na lavoura de arroz, a gente não usa fogo pra nada. O nosso trabalho é com a terra e com a água.
-> END

=== dialogo_enchente ===
# speaker: Expert 
Mr. Lauro, looking at the images from last May's flood and the simulation for this year, we can see the water doesn't rise uniformly. 
# speaker: Expert 
It pushes forward with more force through this strip here, along the bank, before spreading to the rest of the fields. It's as if there's a preferential path for the flood. Do you recognize this pattern?
#speaker: Lauro
The water always comes in through there first, with a force like a bursting channel. It's always the first place where the levees break. We fix them, but in the next flood, it's the same story all over again.
-> END

=== dialogo_final ===
# speaker: Expert 
Mr. Lauro, I believe the images are telling us the whole story. That "preferential path" the flood uses to invade your fields is the exact same area where the riverside vegetation has been cleared. 
# speaker: Expert 
That native growth on the bank isn't a waste of space; it's your first line of defense. The roots hold the bank and soil together, and the dense plants act as a natural breakwater, slowing the water's speed and reducing its force. 
# speaker: Expert 
Without it, the river rushes in without any barrier, breaking your levees and destroying the crops.
#speaker: Lauro
(He looks at the "clear" riverbank, then at his newly repaired levees, thoughtfully) You don't say... We've worked our whole lives to keep this bank clear, thinking we were doing the right thing... 
#speaker: Lauro
And you're telling me that's what was leaving the door wide open for the river? So what can I do now? I need to protect my fields, but without losing my planting area.
-> END

=== veredito_certo ===
    Excellent! Mr. Lauro's problem is the raw power of the river eroding his land and destroying his crops. Soil Bioengineering is the perfect solution because it uses a living defensive system of plants to create natural barriers. 
    These "root walls" and "green breakwaters" will strengthen the riverbank and reduce the flood's force, protecting his levees and his rice fields for years to come.
-> END

=== veredito_errado ===
Oops! That’s not quite right. While those are excellent techniques, Mr. Lauro's main problem isn't soil fertility or inefficient water distribution. 
His farm is being physically destroyed by the force of the river during floods. He needs a solution that can protect his land from being washed away.

-> END




