# NARRATIVE CONTENT AUDIT — C4D

Reauditoria pós-C4A, C4B e C4C. Escopo restrito a conteúdo/documentação; nenhum diálogo novo, gameplay, mapa ou visual foi alterado.

## Contagem real

Fonte: 70 `DialogueData` em `Assets/Resources/Dialogue`; `TOTAL_DIALOGUE_ENTRIES` conta cada item de `lines[]`.

| Medida | C1 | C4D | Delta |
|---|---:|---:|---:|
| TOTAL_DIALOGUE_ENTRIES | 144 | 185 | +41 |
| MELANTHA_LINES | 92 | 117 | +25 |
| POE_LINES | 1 | 14 | +13 |
| ANDREALPHUS_LINES | 35 | 36 | +1 |
| ENVIRONMENT_INTERACTIONS | 29 | 51 | +22 |
| PUZZLE_FEEDBACK_ENTRIES | 28 | 45 | +17 |

`MELANTHA_LINES` inclui 1 `Voz de Melantha`; `POE_LINES` inclui 8 falas literais de Poe e a voz inferida do poço. Narração/voz ambiental ficam fora das colunas de personagens.

## Status por cena

| Cena | Status | Leitura funcional |
|---|---|---|
| QUARTO | CONTENT_SUFFICIENT | Introdução, conflito de memória/ausência, objetos, ritual, bloqueios, feedback e transição onírica. |
| LABIRINTO | CONTENT_SUFFICIENT | Poe tem sinal/voz, revelação, reações e progressão; Ecos têm erro/progresso/conclusão; Andrealphus tem função clara; portal fecha a transição. |
| DOMINIO_LUA | CONTENT_SUFFICIENT | Jardim, espelhos, corredor e sigilo têm introdução/pista, erro, progresso, sucesso e mudança de estado; Poe participa por voz/reação. |
| FINALBETA | CONTENT_SUFFICIENT | Reconhece o fragmento, reage com Melantha/Poe, encerra a slice, declara continuidade, apresenta créditos e habilita retorno. `F04` segue visual-dependent. |

## Poe e Andrealphus

`POE_NARRATIVE_STATUS = SUFFICIENT` — presença no Labirinto, sinal vocal inequívoco, revelação, reações aos Ecos/Andrealphus, portal, Lua, puzzles e encerramento. Não depende de explicar a cosmologia.

`ANDREALPHUS_NARRATIVE_STATUS = SUFFICIENT` — apresentação, função de observador/interrogador, relação com Melantha e Poe, progressão e encerramento da participação na slice. Não há expansão recomendada por volume.

## Feedback de puzzles

| Puzzle | Status | Cobertura |
|---|---|---|
| Jardim | CONTENT_SUFFICIENT | INTRO/HINT, erro por contador/reação, progresso e sucesso. |
| Espelhos | CONTENT_SUFFICIENT | INTRO, erro sem reset arbitrário, progresso parcial, estados de Poe e conclusão. |
| Corredor Ilusório | CONTENT_SUFFICIENT | Regra, repetição/erro, pista indireta, progresso e saída. |
| Sigilo Fragmentado | CONTENT_SUFFICIENT | Pistas, erro, progresso por anel e sucesso com reconhecimento do fragmento. |

## FinalBeta/F04

FinalBeta é fragment-gated, executa a sequência narrativa, mostra encerramento, habilita créditos/retorno e preserva continuidade sem encerrar a história inteira. Retorno ao menu e nova sessão limpa estão cobertos pelo fluxo técnico.

`F04 = VISUAL_DEPENDENT_PENDING`: requer animação/evento visual pós-coleta. Não é falha narrativa enquanto o restante do encerramento estiver suficiente.

## Fechamento C2

| Categoria | Resultado |
|---|---:|
| TOTAL_PLANNED_NEW_ENTRIES | 45 |
| PLANNED_ENTRIES_IMPLEMENTED | 32 |
| INDEPENDENT_ENTRIES_IMPLEMENTED | 32 |
| VISUAL_DEPENDENT_PENDING | 9 |
| OPTIONAL_DEFERRED | 3 |
| DROPPED_WITH_REASON | 1 |

Implementados: C4B `L02,L06,L07,L08,L12,L13,L15`; C4C `D01,D04,D05,D06,D07,D08,D09,D10,D11,D12,D13,D15,D16,D17,D18,D20,D21,D22`; C4A `F01,F02,F03,F05,F06,F07,F08`.

`L01` foi absorvido por `DLG_LABYRINTH_WAKE` e não duplicado. Pendentes visuais: `L04,L05,L10,L14,D02,D03,D14,D19,F04`. Opcionais adiados: `L03,L09,L11`.

## Dependências da Luiza para C6

| ID | Cena | Momento | Visual necessário | Integração posterior |
|---|---|---|---|---|
| L04 | Labirinto | marca ambiental | prop/símbolo final | ligar trigger à marca publicada |
| L05 | Labirinto | parede sem símbolo | posição final | configurar interação sem revelar rota |
| L10 | Labirinto | porta falsa, repetição | posição final | ligar feedback ao binding final |
| L14 | Labirinto | limiar do portal | posição final | integrar fala antes da travessia |
| D02 | DominioLua | leitura do espaço | posição final | ligar trigger de ambientação |
| D03 | DominioLua | pista inicial do jardim | canteiro/prop final | integrar inspeção ao prop |
| D14 | DominioLua | espelhos concluídos | animação final | disparar reação após animação |
| D19 | DominioLua | leitura do sigilo | sigilo/anel final | ligar inspeção à configuração final |
| F04 | FinalBeta | consequência do fragmento | animação/evento final | integrar antes de F05 |

`C6` deve reabrir somente estes IDs após as entregas visuais. `NEXT = C5_OBJECTIVE_UI_AUDIO_MENUS`.
