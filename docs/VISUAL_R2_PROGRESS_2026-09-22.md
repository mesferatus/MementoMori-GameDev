# Integração visual R2 — 22/09/2026

## Implementado nesta retomada

- Confirmados 77 sprites individuais importados (PPU 32, Point, sem mipmaps), registrados em `TestResults/VisualRebuild/r2-imports.tsv`.
- Quarto: cama encostada à parede esquerda, janela de três unidades na parede norte, novos móveis e props. Colisores dos móveis e interações da cama/janela reposicionados. Janela e baú observam `RoomWindowSecured` e `RoomRitualItemStored`, sem alterar progressão.
- Ritual: tapete inativo/ativo e velas apagadas/acesas ligados ao controlador existente.
- Melantha: três clips de caminhada receberam quatro quadros R2 a 10 Hz, loop de 0,4 s; mantidos controlador e parâmetros.
- Labirinto: dez props substituídos, incluindo porta falsa, poço e braseiros. Porta e poço observam flags existentes. Selos dos sete domínios preservados.
- Domínio: dezessete props substituídos; flores observam prontidão e sequência existentes, pedestal responde à coleta, Espelho Negro possui três rachaduras configuradas. Seis molduras novas envolvem os reflexos originais, preservando suas pistas.
- FinalBeta: dezesseis props sincronizados com o Quarto, mantendo o ritual inativo no retorno.
- Comparação antes/depois dos Tilemaps confirmou preservação durante a integração de cada ambiente.

## Verificado

- EditMode: 25/25, relatório `TestResults/VisualRebuild/editmode.xml` desta rodada.
- PlayMode: 12/12, relatório `TestResults/VisualRebuild/playmode.xml` desta rodada.
- Percurso automático no Editor: 11/11, `TestResults/VisualRebuild/r2-ct-editor.json`; ritual, transições, jardim, espelhos, galeria, sigilo, fragmento e encerramento.
- Captura da câmera do Quarto confirmou visualmente o novo encaixe da janela e cama.
- Build Windows R2 criada em `Builds/Windows-Visual-R2-20260922/MementoMori.exe`: zero erros e 518 avisos (incluem variantes de compute shaders; triagem completa pendente).
- Percurso automático da nova build Windows: 11/11, `TestResults/VisualRebuild/r2-ct-windows.json`, execução de 23/09 às 02:55 UTC. Executado sem renderização; não comprova a apresentação gráfica da build. A primeira tentativa restrita falhou ao gravar PlayerPrefs; a segunda execução com acesso às preferências locais passou.

O percurso automático usa serviços e deslocamentos programáticos em partes da verificação. Não é um playtest humano completo. Os testes existentes tampouco comprovam cada novo estado visual por captura.

## Ainda pendente para concluir o prompt inteiro

- Aprovação visual de todas as cenas e de cada estado de interação, incluindo ausência de saltos entre sprites e colisão com todos os móveis.
- Revisão em movimento da nova caminhada, transições para idle e ações especiais; revisão final de Poe e Andrealphus.
- Auditoria dos assets preparados mas ainda não usados: não criar gatilhos novos somente para consumir variações decorativas.
- Conferir composição e leitura de todas as salas do Labirinto/Domínio, HUD, pausa, créditos e encerramento contra as referências.
- Revisar graficamente o novo executável Windows e triar os avisos da build. A progressão automática já foi verificada nesta build.

STATUS FINAL: integração avançada, ainda não concluída integralmente.

PRONTO PARA REVISÃO FINAL DA CALLISTO: NÃO.
