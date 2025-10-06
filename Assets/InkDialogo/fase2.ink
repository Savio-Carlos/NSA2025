-- VARIÁVEIS DA FASE ---
// 1. Variável para o estado geral da fase (controlada pelo C#)
VAR fase_status = "Inicio" 
// 2. Variáveis booleanas para cada descoberta (controladas pelo C#)
VAR encontrou_vegetacao = false
VAR encontrou_umidade = false

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
Good morning, Mr. Jonas. I'm here as we discussed.
#speaker: Jonas
Good morning. Thanks for coming here. The fields look fine from a distance, but up close, the problem is still here.
# speaker: Expert 
You mentioned on the phone you were dealing with some “patches” of low productivity, 
even with the irrigation. Could you show me? 
#speaker: Jonas
Yes, that’s right. They’re not exactly patche , more like strips or corridors. The center pivot spreads water evenly, we use top-quality fertilizer, the seed is all the same… but in those strips, the plants just fail to thrive. I’ve tried everything.

# speaker: Expert 
I understand your frustration, Mr. Jonas. Sometimes the problem isn’t what’s on the plant, but how the plant is interacting with the soil. 
# speaker: Expert 
That’s what this technology is for. We’re going to use satellite imagery to take an “x-ray” and understand what’s really happening beneath the surface. 
#speaker: Jonas
Perfect. Show me what you’ve got. 
-> END

= andamento
#speaker: Jonas
What’s up? 
+ Not yet Mr.Jonas, I will come back when I find something. -> DONE
+ {encontrou_vegetacao} > Confirme the low-productivity areas with the vegetation map.
-> dialogo_vegetacao
+ {encontrou_umidade} > Check if the weak areas are under water stress. -> dialogo_umidade
// Esta escolha só aparece quando TODAS as observações forem verdadeiras
* {encontrou_vegetacao and encontrou_umidade} > Connect the weak areas with pressure loss in the irrigation system. -> dialogo_final
-> END

= final
#speaker: Jonas
Thank you for the help!
-> END

= generico 
#speaker: Jonas
Hmmm, I don’t have nothing new to say right now…
-> END

=== dialogo_vegetacao ===
# speaker: Expert 
Mr. Jonas, the satellite confirms exactly what you said. Look here at the crop health ma : we have these areas in dark green, which are very healthy, but right in the middle of them, we have these strips in light green, almost yellow. 
# speaker: Expert 
The pattern isn’t random; it seems to form corridors. Do you recognize these paths?
# speaker: Jonas
Of course I recognize them! Those strips are where the plants don’t thrive. You can see it clearly from above. I wonder why that happens.
-> END

=== dialogo_umidade ===
# speaker: Expert 
Now, here’s the most intriguing part. This other map measures the moisture in the plants. The areas that were weak on the previous map are the same ones that appear in red here. 
# speaker: Expert 
This means that even with the pivot irrigator running, the plants in these corridors are thirsty; they aren’t managing to absorb the water properly. 

# speaker: Jonas
Thirsty? But I run the pivot every day! The water is there, I can see the ground is wet. How can the plants be thirsty with damp soil?
-> END

=== dialogo_final ===
# speaker: Expert 
Mr.Jonas, I believe the mystery is solved, and the answer was combining the satellite maps with your farm’s design. The strips where your plants are thirsty are the ones farthest from the water pump. 
# speaker: Expert 
In a large irrigation system like yours, it’s common to have pressure loss along the pipeline. They wet the surface, that’s why you see damp soil, but the water application rate is insufficient to penetrate the soil and meet the crop’s needs. 
# speaker: Expert 
Your problem isn’t with fertility; it’s with hydraulic engineering. 

# speaker: Jonas
You mean the water pressure gets weak by the end of the line? Wow, we calibrate the nozzles, but you never think about the pressure water loses along the way… 
# speaker: Jonas
That’s why the crop starts strong and ends weak. And this… can it be fixed? Is it possible to get more pressure into this system? 
-> END

=== veredito_certo ===
Exactly right! The satellite images pinpointed the problem: the plants in the weak  corridorsare thirsty due to a loss of pressure in the irrigation system. 
Smart irrigation solves this hydraulic engineering issue by replacing the sprinklers at the end of the line with models designed to work efficiently with low pressure. 
This ensures that every plant receives the correct amount of water, eliminating the weak strips and maximizing the entire harvest.  
-> END

=== veredito_errado ===
Oops! That’s not quite right. Mr. Jonas’s problem isn’t related to nutrient loss or erosion from floods. The satellite data shows his crops are thirsty in very specific strips, even though the ground is wet.
The issue lies with how the water is being delivered. That’s why Smart Irrigation is the best solution.
-> END
