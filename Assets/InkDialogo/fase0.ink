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
# speaker: Técnico 
Bom dia, senhor. Sou o técnico. Essa é sua propriedade? É uma terra muito bonita.
#speaker: Fazendeiro
(Suspira, sem desviar o olhar da plantação) Bom dia. Sou Arlindo. Ela já foi mais bonita, mas hoje em dia ta cansada.  
# speaker: Técnico 
Cansada? Como assim?
#speaker: Fazendeiro
Antigamente, a gente plantava e a colheita era farta. Dava pra alimentar a família e ainda vender um bom tanto na feira. Agora... mal dá pra gente. Não dá fruto. 
# speaker: Técnico 
E o que o senhor acha que está acontecendo? A chuva foi pouca?
#speaker: Fazendeiro
Que nada! Chuva deu normal. O problema é a terra mesmo. Parece que ela perdeu a força. A gente planta uma, duas vezes no mesmo lugar, e pronto. Acabou-se. 
#speaker: Fazendeiro
Ela não dá mais nada de bom. Aí o jeito é procurar outro canto pra plantar.
# speaker: Técnico 
Seu Arlindo, eu posso te ajudar! Se o senhor me permitir, eu gostaria de dar uma olhada na sua terra usando imagens de satélites . Talvez a gente consiga uma pista do que está enfraquecendo o solo.
#speaker: Fazendeiro
É claro! Eu ficaria feliz com sua ajuda. 
-> END

= andamento
#speaker: Fazendeiro
E então? O que o bicho lá de cima disse?
+ Ainda não encontrei nada. -> END
+ {encontrou_vegetacao} > Falar sobre a diferença da vegetação nos meses de maio, julho e agosto. 
-> dialogo_vegetacao
+ {encontrou_fogo} > Falar sobre a diferença dos focos de calor nos meses de maio, julho e agosto. -> dialogo_fogo
// Esta escolha só aparece quando TODAS as observações forem verdadeiras
* {encontrou_vegetacao and encontrou_fogo} > Conectar a queda da vegetação com o aumento progressivo das queimadas e o problema da "terra cansada". -> dialogo_final
-> END

= final
#speaker: Fazendeiro
Obrigado pela ajuda bixo la de cima!
-> END

= generico 
#speaker: Fazendeiro
Hmm, não tenho nada novo para te dizer agora.
-> END

=== dialogo_vegetacao ===
# speaker: Técnico 
Em todos os anos, a cobertura vegetal em maio é muito saudável, em julho há uma considerável piora, em agosto há diversos lugares com vegetação pouca ou nenhuma vegetação. Qual a razão para essa queda drástica na vegetação no meio do ano?      
#speaker: Fazendeiro
Em maio, saímos das chuvas, então o mato está alto e verde. Nessa época, a umidade é alta, mas conforme a estação avança, essa vegetação toda vai perdendo água. Em julho, ela já está seca e é quando iniciamos o manejo, limpando as pastagens e preparando o solo, o que explica essa piora que você notou.
-> END

=== dialogo_fogo ===
# speaker: Técnico 
Observando o mapa de focos de calor, vemos uma atividade de queimadas muito intensa em agosto de 2020 e quase nenhuma em maio e julho. Por que essa diferença tão grande entre os meses? 
#speaker: Fazendeiro
Isso é o esperado para a nossa região. Maio é um mês úmido, final da estação das chuvas, então o risco de incêndio é baixo. Em julho, nós fazemos o manejo e limpeza das pastagens. Já em agosto, o material vegetal está seco e é o período em que usamos o fogo para limpeza de pastagens e preparação do solo.
-> END

=== dialogo_final ===
# speaker: Técnico 
Em maio, a vegetação está no auge e não há queimadas. Mas conforme ela seca e desaparece entre julho e agosto, 
# speaker: Técnico 
os focos de calor aparecem exatamente no lugar dela. O senhor me disse que a terra fica "cansada" e precisa procurar um novo lugar... 
# speaker: Técnico 
Será que essa prática de usar o fogo para limpar o terreno todo ano não é justamente o que está, pouco a pouco, "cansando" e enfraquecendo o solo?
#speaker: Fazendeiro
Rapaz, a gente sempre faz a queima. É o jeito de limpar tudo e preparar para a nova semente. A cinza ajuda a adubar, é o único jeito que eu conheço de conseguir plantar nesse solo, mas se esgota em 2 a 3 colheitas. Não gosto de queimar, mas não sei como utilizar melhor meu solo. 
-> END

=== veredito_errado ===
Oops! That’s not quite right. Mr. Jonas's problem isn't related to nutrient loss or erosion from floods. The satellite data shows his crops are thirsty in very specific strips, even though the ground is wet. 
The issue lies with how the water is being delivered. That's why Smart Irrigation is the best solution.
-> END

=== veredito_certo ===
Exactly right! The satellite images pinpointed the problem: the plants in the weak corridors are thirsty due to a loss of pressure in the irrigation system. 
Smart Irrigation solves this hydraulic engineering issue by replacing the sprinklers at the end of the line with models designed to work efficiently with low pressure. 
This ensures that every plant receives the correct amount of water, eliminating the weak strips and maximizing the entire harvest.
-> END




