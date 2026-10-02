# Memento Mori V3 — fechamento funcional

Data: 16/09/2026. Escopo desta retomada: validação técnica e build, sem reconstrução visual, arte nova, commit ou push.

## Resultado

**Fluxo automatizado aprovado no Editor e no executável Windows: 11/11 em ambos.** Inclui MainMenu → Quarto → Labirinto → DominioLua → FinalBeta → MainMenu, ritual, transição onírica, Poe, Ecos, Jardim, espelhos, galeria, sigilo, fragmento, pausa e conclusão dos diálogos de encerramento.

O CT aciona os serviços/interações do jogo e percorre fisicamente a rota do Jardim; também reposiciona o jogador em outras etapas. Esta evidência é de fluxo automatizado, não de playthrough humano integral.

## Testes e evidências

| Verificação | Resultado | Evidência |
|---|---|---|
| EditMode | 25/25 na última execução, sem alterações posteriores de lógica nesta retomada | job `afa5b3d456fb`; relatório V3_CLOSURE_REPORT.md |
| PlayMode | 12/12 na última execução, sem alterações posteriores de lógica nesta retomada | job `f8dda1c256ee`; relatório V3_CLOSURE_REPORT.md |
| CT Editor, repetido nesta retomada | 11/11 | `TestResults/ct-functional-editor.json` |
| CT Windows | 11/11; saída 0 | `TestResults/ct-functional-windows-authorized.json`, `functional-windows-authorized-exit.txt` |
| Menu | Jogar, Créditos e Sair ativos, interativos e com ação vinculada | `TestResults/functional-ui-menu.txt` |
| Objetivo e prompt | objetivo atualizado para “Descanse.”; duas exibições registradas; prompt vinculado ao CanvasGroup e TMP | `TestResults/functional-ui-runtime.txt` |
| Diálogo e encerramento | sequência completa de FinalBeta terminada | CT-011 nos dois ambientes |
| Áudio | ambiente do Quarto tocando; 20 WAVs anteriormente verificados como legíveis | `functional-ui-runtime.txt`; relatório V3_CLOSURE_REPORT.md |
| Erros de execução Windows | nenhuma ocorrência de exceção/erro crítico na execução aprovada | `TestResults/functional-windows-authorized.log` |

## Build entregue

- Pasta exclusiva: `Builds/Windows-Functional-V3-20260916/`.
- Executável: `MementoMori.exe`.
- Windows x64, Mono, Development (inclui o CT opcional).
- Resultado: **Succeeded**, **0 erros**, 518 avisos; tamanho informado pelo Unity: 350.357.130 bytes; duração: 3min57s.
- Avisos concentrados em shaders Sentis/ConvGeneric, APIs obsoletas e campos não utilizados. Não bloquearam o build nem o fluxo executado.
- Resumo: `TestResults/build-functional-summary.txt`.
- SHA-256 do EXE: `C2F1EC702A37D272F5AA869DB417D533BC59EB7ABB9CE16898C0471454DF8850`.
- Builds anteriores preservados. Para jogar, abrir o EXE sem o argumento de CT e manter as pastas e DLLs adjacentes.

## Restrição de ambiente encontrada e resolvida

A primeira execução dentro do ambiente restrito falhou em `PlayerPrefs.SetString`, impedindo salvar o checkpoint e interrompendo a transição. Evidências preservadas em `ct-functional-windows.json` e `functional-windows-player.log`.

A mesma build foi executada com permissão para gravar as preferências do Windows. Passou 11/11, terminou com código 0 e não apresentou a exceção. Nenhuma alteração no jogo foi feita para contornar essa restrição do teste.

## Mudanças nesta retomada

- Configuração `PlayerSettings.runInBackground=true`, para execução continuar sem foco; build usa as cinco cenas oficiais e o backend Mono já adotado no projeto.
- Build e evidências em pastas separadas.
- Este relatório e atualização do relatório anterior.
- Nenhum script de gameplay, sprite, mapa, fonte ou interface foi reconstruído nesta retomada. Nenhum bug funcional adicional exigiu correção de código.

## Pendências e limite da conclusão

Não foi identificado bloqueio funcional nos testes executados. Playthrough humano integral e aprovação auditiva/artística não foram realizados nesta retomada. Os avisos de compilação permanecem registrados. A versão está pronta para iniciar a etapa visual completa em outra sessão.
