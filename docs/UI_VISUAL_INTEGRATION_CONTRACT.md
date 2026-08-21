# C5.2 — Contrato de integração visual da UI

## Regra central

A Luiza pode substituir apresentação, layout, sprites, fontes, frames, ícones, cores e animações. Não pode remover controllers, EventSystem, eventos, referências funcionais, nomes/IDs ou scripts.

| UI_ID | CENA / OBJETO | VISUAL_SUBSTITUÍVEL | DONO FUNCIONAL QUE DEVE PERMANECER | BINDINGS / IDs A PRESERVAR | GATE |
|---|---|---|---|---|---|
| UI01 | MainMenu / `MainMenuCanvas` | fundo, logo, título, botões, fonte | `MainMenuController` | Jogar, Créditos, Sair, Voltar | cena abre e botões funcionam |
| UI02 | Créditos / `Credits` | painel, tipografia, rolagem, frames | `MainMenuController` | abrir/fechar créditos | retorno ao menu |
| UI03 | Pause / `PauseMenu`, `PausePanel` | painel, botões, ícones | `PauseMenuController`, `InputGate` | continuar, reiniciar checkpoint, menu | input bloqueia/restaura |
| UI04 | Dialogue Box / `DialogueManager`, `DialoguePanel`, `DialogueHistory` | caixa, speaker, texto, histórico, animação | `DialogueManager` | referências de texto, avançar, fechar | diálogo completa sem travar |
| UI05 | Interaction Prompt / `InteractionPrompt` | label, ícone, moldura, animação | `InteractionPromptUI` | target/contexto e atualização | aparece apenas com candidato |
| UI06 | Objective Toast / `Objective`, `Hint` | toast, ícone, cor, duração | controller de objetivo existente na cena | evento de atualização | não intercepta input |
| UI07 | FinalBeta / `FinalCanvas`, `FinalText`, `Credits` | encerramento, animação e créditos | `FinalBetaController` | `finalText`, `credits`, retorno | fragmento e retorno funcionam |
| UI08 | Botões/fontes/frames/ícones/cores | todos os assets de apresentação | scripts, `EventSystem`, `Button.onClick` | nomes e listeners | sem Missing References |

## LUIZA_MUST_PROVIDE

Skins e prefabs visuais editáveis, com anchors/layout/sorting coerentes, estados normal/hover/pressed/disabled, legibilidade, fonte e contraste adequados. Entregar as referências dos objetos funcionais preservadas e uma lista de qualquer alias alterado.

## CALLISTO_MUST_CONNECT

Revalidar controllers, `Button.onClick`, `EventSystem`, `InputGate`, referências de texto/imagem, diálogo, prompt, objetivo, pause, créditos e FinalBeta; executar smoke de abrir/fechar e regressão do fluxo.

## Proibições

Não remover `MainMenuController`, `PauseMenuController`, `DialogueManager`, `InteractionPromptUI`, `FinalBetaController`, `EventSystem`, bindings, IDs, scripts ou referências apenas para aplicar a skin. Não converter a UI em imagem achatada.

`UI_READY_CONTRACT = YES`.

## FinalBeta — F04

| ITEM | VISUAL/ANIMAÇÃO NECESSÁRIA | QUANDO TOCA/APARECE | EVENTO LÓGICO PRONTO | LUIZA FORNECE | CALLISTO FAZ DEPOIS |
|---|---|---|---|---|---|
| F04 | consequência visual do Fragmento da Alma: brilho/animação de encerramento e transição para a apresentação final | após coleta do fragmento e antes de F05/texto final | `FragmentCollected` e entrada em `FinalBeta` já estão no fluxo | animação, efeitos, duração e pontos de referência, sem placeholder | conectar o hook, sincronizar `FinalBetaController`/áudio/dialogue e testar retorno |

`F04` continua `VISUAL_DEPENDENT_PENDING`; não implementar placeholder neste contrato.
