// --- VARIÁVEIS DA FASE ---
// 1. Variável para o estado geral da fase (controlada pelo C#)
VAR fase_status = "Inicio" 
// 2. Variáveis booleanas para cada descoberta (controladas pelo C#)
VAR encontrou_vegetacao = true
VAR encontrou_fogo = true
VAR encontrou_enchente = true

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
# speaker: Inspetor 
Bom dia, senhor. Sou o inspetor que você contratou. Essa é sua propriedade? É uma bela lavoura de arroz.
#speaker: Fazendeiro
Bom dia, sou o Lauro. Obrigado, eu me esforço bastante. A gente tenta, mas ultimamente tá difícil manter ela bonita. O rio não anda mais pra brincadeira. 
# speaker: Inspetor 
Imagino o trabalho para reconstruir depois da última enchente. Foi muito grave por aqui?
#speaker: Fazendeiro
Grave? A água passou por cima de tudo. Levou taipa, estragou bomba d'água, afogou a lavoura que estava quase no ponto de colher. Grave é pouco. Antigamente, a gente conhecia o rio, sabia até onde ele vinha na cheia. Agora, ele vem com uma fúria que nunca vi. Cada vez que o céu escurece, a gente já fica de orelha em pé.

# speaker: Inspetor 
E o que o senhor acha que mudou? É só o volume da chuva?
#speaker: Fazendeiro
A chuva vem um temporal, é verdade. Mas não é só isso. Parece que a terra não segura mais a água. A chuva bate e escorre direto pro rio, que vira um monstro em poucas horas. E quando ele sobe, não tem taipa que segure. A proteção da terra se foi.
# speaker: Inspetor 
Seu Lauro, talvez eu possa ajudar. Com as imagens de satélite que tenho aqui, podemos analisar como a água se comportou na sua propriedade e entender melhor os pontos vulneráveis. Se o senhor permitir, podemos olhar juntos.
#speaker: Fazendeiro
É claro! Tudo que puder me ajudar a não passar por aquele pesadelo de novo, eu quero ver.
-> END

= andamento
#speaker: Fazendeiro
E então? O que o bicho lá de cima disse?
+ {encontrou_vegetacao} > Mostrar a diferença de vegetação entre a lavoura e a margem do rio.
-> dialogo_vegetacao
+ {encontrou_fogo} > Comentar sobre a ausência de queimadas na área. -> dialogo_fogo
+ {encontrou_enchente} > Falar sobre a mancha de inundação e como ela avançou.-> dialogo_enchente
// Esta escolha só aparece quando TODAS as observações forem verdadeiras
* {encontrou_vegetacao and encontrou_fogo} > Conectar a falta de mata ciliar com o caminho da enchente e a vulnerabilidade da lavoura. -> dialogo_final
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
# speaker: Inspetor 
Agora, cruzando com os mapas de vegetação, note algo interessante. Em maio, antes da enchente, sua lavoura estava saudável, com um verde forte. Mas nesta mesma faixa por onde a água avança, a vegetação nativa é muito rala, quase inexistente. Em junho, depois da enchente, essa área fica completamente "lavada". Por que essa beira de rio é tão desprotegida? 
#speaker: Fazendeiro
Essa terra na beira do rio sempre foi a mais fácil de trabalhar e boa de plantar. A gente sempre se manteve limpa pra aproveitar o máximo de espaço pra lavoura de arroz. Deixar o mato crescer ali sempre pareceu um desperdício.
-> END

=== dialogo_fogo ===
# speaker: Inspetor 
Eu também verifiquei os registros de focos de calor e, como esperado para a sua cultura, não há praticamente nenhuma atividade de queimada na sua propriedade em nenhum dos anos. Seu manejo é todo baseado na água e no preparo mecânico do solo, correto?
#speaker: Fazendeiro
Isso mesmo. Fogo aqui, só na churrasqueira. Na lavoura de arroz, a gente não usa fogo pra nada. O nosso trabalho é com a terra e com a água.
-> END

=== dialogo_enchente ===
# speaker: Inspetor 
Seu Lauro, olhando as imagens da enchente de maio do ano passado e a simulação para este ano, vemos que a água não sobe de forma uniforme. Ela avança com mais força por essa faixa aqui, ao longo da margem, antes de se espalhar pelo resto da lavoura. É como se houvesse um caminho preferencial para a inundação. O senhor reconhece esse padrão?
#speaker: Fazendeiro
A água entra primeiro por ali, com uma força que parece um sangrador. É sempre o primeiro lugar onde as taipas estouram. A gente conserta, mas na cheia seguinte, a história se repete.
-> END

=== dialogo_final ===
# speaker: Inspetor 
Seu Lauro, acho que as imagens estão nos contando a história completa. Aquele "caminho preferencial" que a enchente usa para invadir sua lavoura é exatamente a área onde a mata ciliar foi removida. Essa mata nativa na beira do rio não é um desperdício; ela é a primeira linha de defesa. As raízes seguram o barranco e o solo, e a vegetação densa funciona como um quebra-mar, diminuindo a velocidade e a força da água. Sem ela, o rio sobe e avança sem nenhuma barreira, estourando suas taipas e destruindo a lavoura.
#speaker: Fazendeiro
(Ele olha para a margem "limpa" do rio, depois para as taipas recém-consertadas, pensativo) Capaz... A gente trabalhou tanto a vida toda pra manter essa beirada limpa, achando que tava fazendo o certo... E era justamente isso que tava deixando a porta aberta pro rio entrar? E o que eu posso fazer agora? Preciso proteger minha lavoura, mas sem perder minha área de plantio.
-> END




