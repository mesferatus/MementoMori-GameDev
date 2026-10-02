# Fontes R2 — produção em andamento

Atualização de 22/09: 77 sprites individuais importados em `Assets/Art/FinalProduction/R2`, com PPU 32, filtro Point e transparência. Evidência: `TestResults/VisualRebuild/r2-imports.tsv`. Quarto, Labirinto, Domínio e quarto de retorno receberam integração parcial; relatórios `r2-*-integration.txt` registram o alcance. A integração não equivale a aprovação visual nem a playtest completo.

As observações de halo abaixo são históricas: a inspeção do canal alfa confirmou transparência no entorno das fontes aceitas. A caminhada frente/costas aprovada para importação é `M1_Walk_FrontBack_Corrected_Source.png`, não a primeira tentativa listada abaixo.

Pisos e paredes existentes serão preservados por instrução do usuário. Não aplicar a geração Q1 desta retomada.

| Fonte | Conteúdo | Situação |
|---|---|---|
| Q2_Window_States_Source.png | Janela aberta/segura | Conferir escala 96x64 e alinhamento da moldura |
| Q2_Bed_Source.png | Cama | Conferir footprint 80x154 e encoste na parede |
| Q2_Furniture_Source.png | Mesa, criado, banco, mesa lateral, altar, aparador | Corrigir halo/transparência; depois recortar individualmente |
| Q3_Props_States_Source.png | Retratos, tigelas, brinquedos, livros, baú, frasco, tinteiro | Conferir densidade de pixels e estados realmente consumidos pelo jogo |
| L3_FalseDoor_States_Source.png | Porta falsa aberta/fechada | Conferir 64x96 e pivô comum |
| L3_Details_States_Source.png | Poço, obelisco, pedestal, altar, braseiro, correntes | Conferir medidas de cada objeto e estados |
| D2_Garden_States_Source.png | Flores em pares de estados e arbustos | Halo visível: requer correção antes de aprovação |
| D3_Mirror_Modules_Source.png | Espelhos e moldura modular | Conferir transparência e preservar reflexos dos puzzles |
| D4_BlackMirror_States_Source.png | Espelho Negro e rachaduras | Halo pendente; manter moldura registrada |
| Q4_Ritual_States_Source.png | Velas, tapete e tigelas | Halo pendente; alinhar estados |
| D5_D6_MoonProps_Source.png | Ciclos lunares, pedestal, fragmento e cristais | Conferir recortes e estados |
| M1_Walk_Right_Source.png | Passos laterais separados | Melhor diferenciação contato/passagem; loop ainda não validado |
| M1_Walk_FrontBack_Source.png | Passos frente/costas | Contatos 0 e 2 ainda parecidos; não aprovado |
| M1_Walk_Candidate_Source.png | Primeira tentativa 4x4 | Rejeitada para integração: pernas repetitivas |

Catálogo de recortes e dimensões: `import-catalog.json`. Nenhuma destas fontes foi importada nesta retomada. A Unity respondeu no projeto correto, porta 7890, fora de Play Mode. As chamadas RoomTiles foram removidas dos helpers ApplyRoom e ApplyReturnRoom; nenhuma cena foi alterada por esses helpers nesta retomada.

Os prompts de cada geração foram enviados na conversa. Não ligar variações a novos gatilhos inventados: mapear primeiro o script/flag existente. Estados extras podem ficar preparados sem criar gameplay. As fontes não comprovam integração, transparência válida, playtest ou conclusão.
