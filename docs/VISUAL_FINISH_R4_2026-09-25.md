# Finalização visual R4 — assets existentes

Trabalho iniciado em 24/09 e validado em 25/09/2026, Unity 6000.4.12f1. Base de inventário: `VISUAL_ASSET_AUDIT_2026-09-24.md`; não foi refeita a auditoria. Alterações de cenas e importadores executadas pelo Unity MCP, com Editor aberto. Nenhum commit, push, exclusão ou arquivamento de assets.

## Implementação

| Área | Resultado e evidência |
|---|---|
| Melantha | Locomoção R3 preservada. PPU de 39 quadros antigos calibrado por sequência contra 53 pixels visíveis em PPU 32. Pivô inferior ajustado a 1 pixel dentro do recorte antigo. Ações em pé: PPU 35,01887; ritual/deitar: 43,4717; dormir/acordar/queda: 51,92453. Mesma escala dentro de cada sequência, sem esticar os eixos nem alterar o PNG. Amostragem runtime de idle, walk, interação, leitura, ritual e reação. As diferenças de desenho entre gerações de sprites permanecem. |
| Poe | Arte e Animator preservados. Corrigido movimento reto que atravessava regiões sem piso: rotas cardinais pelo Tilemap e verificação de obstáculos sólidos. Triggers e Player não bloqueiam a rota. Teste específico de corredor em L aprovado. |
| Andrealphus | Arte, Animator, fala e ordenação por posição preservados; composição do núcleo recebeu colunas e candelabros. |
| Quarto | Cama reduzida para 2,55 unidades de largura, mesa para 2,95, criado-mudo para 1,15. Janela posicionada na faixa de parede norte. Tilemap de parede existente reutilizado. Ponto de interação da cama acompanha a nova posição. |
| Labirinto | Área ampliada R3 preservada. Variação de piso, bordas e cantos; pilares, estátuas, obelisco, candelabros, bandeiras, correntes e detalhes existentes distribuídos por entrada, porta falsa, poço, corredores, câmara, núcleo e sete domínios. Seis portais continuam selados; Lua mantém sua progressão existente. |
| Domínio da Lua | Piso e cantos revisados, entrada monumental, árvores/plantas/fonte/bacia de cristal no jardim, marcos para espelhos, galeria, sigilo e portal. Puzzles e suas referências preservados. Capturas do jardim e espelhos em Play Mode. |
| MainMenu | Logo existente, molduras inteiras de botão, espaçamento e ornamentos de canto; validado por screenshot real do Game View. |
| Pause | Fundo escuro opaco, título gótico, botões com molduras inteiras, centralização e legibilidade. |
| Dialogue | Retrato ajustado dentro da moldura, margens e nome gótico. Texto original preservado. |
| Objective | Painel 420×220, área interna 292×90, texto central e quebra de linha. Persistência passou nos testes existentes. |
| Portraits | Nove retratos existentes preservados; escala e anchors ajustados. A troca por personagem existente foi mantida, sem inventar regras narrativas para expressões. |
| FinalBeta | Reutiliza composição revisada do Quarto, Melantha R3, ritual apagado, velas do ritual apagadas e fragmento físico. Cartão final e botões conferidos em Play Mode. |
| Câmera | Pixel Perfect/PPU 32 e acompanhamento existentes preservados. Capturas 3840×2160 conferidas; não foi reduzida a escala dos mapas. |
| Sorting | Novas peças usam contato com o chão e ordem `5000 - y*100`, compatível com `VisualDepthOrder` dos atores. |
| Colliders | Bases dos novos objetos sólidos com caixas estreitas; todos os novos sólidos conferidos sobre células de piso. Colisão do contorno preservada. Rota física do Player até o jardim validada pelo CT. |
| Animações | Clips, nomes, referências e duração preservados; importadores de ações de Melantha corrigidos. Testes de progressão mantêm aparições e encerramento. |

## Assets e arquivos

- **Novos assets gráficos gerados: zero.** Uma tentativa anterior ao pedido revisado falhou por limite de geração; nenhum resultado foi importado. Nenhuma geração após o pedido de reutilização.
- Reutilizados: R3/Melantha, R3/Decoration, R3/LabyrinthTiles, R3/MoonTiles, móveis/estados R2, Q1_RoomTiles32, UI MainMenu e sheets U1/U2/U3/C1.
- Importadores alterados: os 39 quadros de ações/cutscenes de Melantha listados em `TestResults/VisualRebuild/r4-melantha-imports.txt`.
- Recortes: nenhum retângulo, nome ou sprite ID alterado; somente pivôs e PPU desses quadros.
- Cenas: MainMenu, Quarto, Labirinto, DominioLua, FinalBeta.
- Código de jogo: somente `Assets/Scripts/Poe/PoeFollower.cs` nesta rodada. Motivo: travessia do vazio identificada na trajetória registrada em CT, mesmo com progressão aprovada.
- Teste novo: `Assets/Tests/PlayMode/PoeNavigationPlayModeTests.cs`.
- Receitas editoriais: `Tools/ComposeMapsR4.cs.txt` e `Tools/RefinePresentationR4.cs.txt`. Executadas pelo MCP, não são scripts de gameplay.
- Backups anteriores às alterações desta rodada: `ArtStaging/VisualFinalR4/SceneBackups`.
- O projeto já continha muitas alterações locais e arquivos não rastreados; este relatório não atribui essas mudanças anteriores à R4.

## Validação

- **EditMode: 25/25**, após correção de navegação, `TestResults/VisualRebuild/editmode.xml`.
- **PlayMode: 13/13** — baseline 12/12 mais teste novo de navegação, `TestResults/VisualRebuild/playmode.xml`.
- **CT Editor: 11/11**, repetido após correção, `TestResults/VisualRebuild/r4-ct-editor-navigation.json`.
- CT cobre investigação/ritual, transição, Poe, ecos, jardim, espelhos, galeria, sigilo, bloqueio do portal, fragmento, encerramento e retorno ao menu. Usa serviços e interações reais; não equivale a uma partida humana completa nem verifica todos os pixels de cada estado.
- Inspeção visual em Play Mode: menu, Quarto/diálogo, pausa, entrada do Labirinto, jardim, espelhos, retorno e cartão final. Os deslocamentos para jardim/espelhos usados nas capturas foram feitos por posicionamento de inspeção, não devem ser descritos como caminhada manual completa.
- **Build Windows: Succeeded**, Development/Mono, 348.577.298 bytes, 1min39s, zero erros e 521 avisos. Executável: `Builds/MementoMori-validated.exe`. Evidência: `TestResults/VisualRebuild/r4-build-summary.txt`; avisos completos em `r4-build-warnings.txt`.
- **Build testada: CT 11/11**, `TestResults/VisualRebuild/r4-ct-windows-permitted.json`, alvo `WindowsDevelopmentBuild`, erro vazio. A execução abriu o player gráfico em 1280×720 e encerrou após retornar ao menu.
- A primeira execução externa foi impedida de gravar PlayerPrefs pelo ambiente restrito e falhou. Foi repetida com permissão de execução/gravação local, sem alterar código ou testes; então passou. O relatório e log da falha inicial foram preservados em `r4-ct-windows.json` e `r4-player.log`; a execução válida está em `r4-player-permitted.log`.

## Screenshots

Todos em `TestResults/VisualRebuild/`:

- `r4-mainmenu-final.png`
- `r4-quarto.png`
- `r4-pause.png`
- `r4-labirinto.png`
- `r4-lua-jardim.png`
- `r4-lua-espelhos.png`
- `r4-finalbeta-retorno.png`
- `r4-finalbeta-endcard.png`

## Limites e console

O Editor registrou conflito de porta do serviço Unity Pipeline após recarga, aviso de Account API/rede, aviso de Input Manager obsoleto e avisos anteriores de DontDestroyOnLoad. O MCP reconectou na porta 7891. Não afirmar console completamente limpo. Na execução externa permitida, a busca por Exception, NullReference, MissingReference e error não retornou ocorrências.

`git diff --check` amplo apontou espaços finais em metadados Unity que já pertenciam ao conjunto de alterações locais. Não foi feita limpeza desses arquivos fora da rodada. O código de navegação foi compilado e testado; não confundir esse resultado de estilo do diff amplo com erro de compilação.

Os 521 avisos da build incluem APIs obsoletas da Unity 6, Unity Services sem projeto vinculado e ausência de configuração runtime do plugin Pipeline (desabilitado no player). Não foram eliminados por limpeza geral, fora do escopo solicitado. O resultado técnico é compilação sem erro e CT externo aprovado; não é console sem avisos.

A revisão artística de todos os estados de flores, rachaduras, reflexos e transições de cutscene não é substituída pelos testes funcionais. A escala das ações foi normalizada, mas os desenhos antigos e R3 não se tornam idênticos por ajuste de importador. Não foi criada arte para esconder essa diferença.

## Status de entrega

**Pronto para revisão da Callisto: SIM, como versão implementada e testada para revisão visual.** Não representa aprovação artística humana definitiva nem um playthrough manual exaustivo. A revisão deve concentrar-se na densidade decorativa dos mapas, diferença de desenho entre ações antigas e locomoção R3 e estados visuais não cobertos individualmente pelas capturas. Não há falha de teste remanescente nas execuções válidas. Nenhum commit ou push realizado.

