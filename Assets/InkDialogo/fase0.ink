// --- VARIÁVEIS DA FASE ---
// 1. Variável para o estado geral da fase (controlada pelo C#)
VAR fase_status = "Inicio" 
// 2. Variáveis booleanas para cada descoberta (controladas pelo C#)
VAR encontrou_vegetacao = false
VAR encontrou_fogo = false

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
Good morning, sir. I’m the Expert. Is this your property? It’s a beautiful piece of land.
#speaker: Arlindo
Good morning. I’m Arlindo. It used to be more beautiful, but these days it’s worn out.
# speaker: Expert 
Worn out? What do you mean?
#speaker: Arlindo
Back in the day, we planted and the harvest was plentiful. It was enough to feed the family and still sell a good amount at the market. Now… we can barely get a crop out of it.
# speaker: Expert 
And what do you think is happening? Was there not enough rain?
#speaker: Arlindo
Not at all! Rain’s been fine. The problem’s the soil itself — it’s like it’s lost its strength. You plant in the same spot once or twice, and that’s it. 
# speaker: Expert 
Mr. Arlindo, I can help you! If you’ll allow me, I’d like to take a look at your land using satellite images. Maybe we can find some clues about what’s weakening the soil.
#speaker: Arlindo
Of course! I’d be glad to have your help. 
-> END

= andamento
#speaker: Arlindo
Then, do you have anything to say?
+ Not yet Mr.Arlindo, i will come back when i find something. -> DONE
+ {encontrou_vegetacao} > Talk about how the vegetation changes between May, July, and August. 
-> dialogo_vegetacao
+ {encontrou_fogo} > Talk about how fire activity changes between May, July, and August. -> dialogo_fogo
// Esta escolha só aparece quando TODAS as observações forem verdadeiras
* {encontrou_vegetacao and encontrou_fogo} > Connect the drop in vegetation with the rise in fires and the problem of “worn out” soil. -> dialogo_final
-> END

= final
#speaker: Arlindo
Obrigado pela ajuda bixo la de cima!
-> END

= generico 
#speaker: Arlindo
Hmm, não tenho nada novo para te dizer agora.
-> END

=== dialogo_vegetacao ===
# speaker: Expert 
Every year, we can see that vegetation cover looks very healthy in May, but by July it’s noticeably worse. And in August, there are many spots with little to no vegetation left. What could be the reason for such a drastic drop in vegetation around mid-year? 
#speaker: Arlindo
Well, in May we’re just coming out of the rainy season, so everything’s green and full of life. The grass is tall, and the soil still holds plenty of moisture. 
#speaker: Arlindo
But by July, we’ve already harvested and started clearing the fields and preparing the soil to get the land ready for the next planting. That’s why the vegetation looks thin and patchy by mid-year.

-> END

=== dialogo_fogo ===
# speaker: Expert 
Looking at the fire activity maps, there’s intense burning in August 2020, but almost none in May or July. Why is there such a big difference between those months?
#speaker: Arlindo
That’s pretty normal for our region. May is still a humid month, right at the end of the rainy season, so the fire risk is low. In July, we start managing the pastures by clearing and cleaning up the fields. 
#speaker: Arlindo
Then in August, everything’s dry, and that’s when we use fire to clear the land and prepare the soil for planting.
-> END

=== dialogo_final ===
# speaker: Expert 
In May, vegetation is at its peak, and there’s no fire activity. But as it disappears between July and August, those fire spots start showing up right where the vegetation used to be. You mentioned that the land gets “worn out” and you have to move to new areas… 
# speaker: Expert 
Could it be that burning the fields every year is actually what’s wearing out and weakening the soil little by little?
#speaker: Arlindo
You might be right, son. We’ve always burned the fields. It’s the only way we know to clear everything and get ready for the next crop. 
#speaker: Arlindo
The ash helps fertilize the soil, but after two or three harvests, it’s spent. I don’t like burning, but I don’t really know another way to make this soil work.
-> END

=== veredito_certo ===
Well done! Arlindo’s soil isn’t suffering from lack of rain or water, but from nutrient loss over repeated planting and clearing.
Agroforestry is the best solution because it restores fertility naturally, mixes crops that nourish the soil, attracts beneficial insects, and keeps the land productive for many years, allowing sustainable farming without wearing the soil out.

-> END

=== veredito_errado ===
Oops! That’s not quite right. Techniques like Soil Bioengineering or Smart Irrigation can be useful in certain situations. For example, to protect the soil from erosion or to distribute water evenly, but Arlindo’s problem is different.
 His soil is “worn out” because it loses nutrients after a few plantings and harvests. What he really needs is a solution that restores fertility and keeps the land productive for the long term. Try again, thinking about how to bring life back to the soil sustainably.

-> END




