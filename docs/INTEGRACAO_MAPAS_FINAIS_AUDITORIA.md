# Integração dos mapas finais — auditoria de implementação

Estado: **parcial, ainda não validado em jogo**. A branch `integration/mapas-finais` foi criada sobre `master` limpo; o commit original da Luiza foi integrado por cherry-pick com autoria preservada. A geometria e os objetos funcionais das cenas oficiais ainda não foram alterados.

## Medidas e base

- Unity 6000.4.12f1; tile de 1 unidade; sprites de mapa atuais a 32 PPU.
- Labirinto oficial: Tilemap `V3MapArt/Floor` com 3.284 células, limites x -32..31 e y -56..35; `V3MapArt/Walls` e `Collision` com 800 células. A topologia atual tem salas grandes e poucas decisões; não satisfaz ainda os loops solicitados.
- Domínio da Lua oficial: `Floor` com 3.464 células, limites x -32..33 e y -46..39; paredes/colisão com 612 células. Há objetos funcionais para Jardim, Espelhos, Galeria, Sigilo e saída.
- Entrega da Luiza: `Assets/mapa luiza/SampleScene.unity` com 9 Tilemaps, em área aproximada de 133 × 111 células, sem colisores. A composição e a região do jardim são referências de acabamento. Não é uma cena funcional pronta para substituir as oficiais.
- Personagem: collider circular com raio 0,5 unidade; largura livre alvo para Melantha e Poe: 3 tiles.
- Salas bloqueadas oficiais: portais nas coordenadas (-20,-40), (0,-40), (20,-40), (0,-29), (20,-29), (0,-50). Uma composição 16 × 16 unidades sobreposta às posições atuais invade salas vizinhas: é necessário reposicionar a planta antes de instalar os seis cenários grandes.

## Reutilizar

- Geometria funcional, triggers, estados, portais, transições, puzzles e Tilemaps das cenas oficiais como base de edição.
- Tiles `LabyrinthTiles` e `MoonTiles` de 32 × 32, quando a inspeção visual em cena confirmar coerência com as referências aprovadas.
- Estados visuais e lógica existentes dos Espelhos, Galeria, Sigilo e Portal, sujeitos a verificação de leitura e sequência.
- Composição orgânica do jardim da Luiza como referência interna; reaproveitamento direto de sprites depende da licença de cada pacote.

## Adaptar / refazer

- Labirinto: ampliar a rede a partir dos Tilemaps oficiais com 2–3 loops, 4 desvios e 7 decisões verificáveis; paredes, cantos, passagens, limiares e colisores devem acompanhar cada nova célula. Conservar os marcos existentes.
- Domínio da Lua: aproveitar a geometria da Luiza onde ela permitir encaixar os sistemas oficiais, mantendo Entrada, Jardim, Espelhos, Corredor Ilusório, Galeria, Sigilo, Fragmento e saída ligados por rotas navegáveis.
- Ajustar decoração, pilares, portas, chamas e ornamentação que destoarem da referência, sem substituir GUID de sprites 1:1 enquanto houver dependentes.
- Reposicionar as seis salas bloqueadas com espaçamento mínimo de 16 tiles entre centros mais margem para corredores. Manter portão, fechadura, VFX e collider como objetos separados.
- Corrigir Poe somente após finalizar a planta. O `PoeFollower` já possui busca em grade; validar primeiro mapa de piso, colisão, partida no trigger, recuperação e loops.

## Gerar novo

- **L2 — seis salas cenográficas:** folha RGBA 1536 × 1024, seis quadros de 512 × 512, cada um equivalente a 16 × 16 unidades a 32 PPU. Importada e recortada em `L2_Devil`, `L2_Tower`, `L2_Hanged`, `L2_Hermit`, `L2_Death`, `L2_Judgement`. Cada composição contém identidade própria em pedra gótica escura. Os sprites estão preparados, mas ainda não instalados na cena.
- **D2 — jardim substituto:** folha original RGBA 1254 × 1254, recortada em 64 sprites de uma unidade. Ainda não aplicada aos 62 Tile assets dependentes de `Gentle Forest`; a correspondência visual entre piso, bordas, obstáculos e vegetação deve ser feita por função antes da substituição.
- **Labirinto modular complementar:** somente após comparar em cena os tiles atuais e os da Luiza. Se necessário: piso base/gasto/rachado, bordas, paredes retas, cantos internos/externos, T/cruzamentos, arcos, escadas, pilares, correntes, suportes de tochas, sigilos e marcadores discretos.
- **Lua modular complementar:** somente lacunas reais: mosaico, piso rachado, bordas, cantos, pilares, arcos, sigilos e ruína pequena. Jardim, espelhos e VFX devem permanecer individuais quando precisarem de interação, animação ou camada própria.

## Substituir no mapa

1. Congelar um inventário por cena, objeto/Tilemap, sprite atual, classificação e destino; CSV de trabalho: `SPRITE_REPLACEMENT_MANIFEST.csv` na pasta de evidências local. Classificações automáticas são provisórias até inspeção visual.
2. Desenhar a nova circulação e medir o percurso em células; conferir largura livre, loops, becos e acessibilidade de todos os marcos.
3. Atualizar `Floor`, `Walls` e `Collision` no Editor, mantendo Tilemaps e objetos existentes. Não usar as imagens de referência como background.
4. Encaixar as seis salas L2 na planta expandida, sem alterar o `MoonPortal` acessível; porta/lock, chamas, foreground e colisão permanecem independentes.
5. Ajustar posições dos triggers e validar os estados: Lua aberta, seis bloqueados; Espelhos `Delayed → Ahead → Absent`; Sigilo `Minguante → Grimório → Sustentar`; corredor com três escolhas e retorno no erro.
6. Validar transição, Poe, Fragmento, colisões, câmera e fluxo completo em Play Mode antes de marcar como concluído.

## Dependência de direitos dos assets

A entrega da Luiza contém o arquivo-fonte de `Gentle Forest` de Seliel the Shaper. A licença publicada pelo autor permite trabalho em equipe, restringe redistribuição do pacote e proíbe usá-lo em projeto junto com material gerado por IA. Como L2 é arte gerada nesta rodada, a integração final e o push dependem de autorização específica do autor ou substituição integral desse pacote antes de publicar a branch. Os demais pacotes da entrega também precisam de verificação individual. Não tratar a presença do arquivo no repositório como prova de licença.

Callisto escolheu **substituir o pacote**. O commit original da Luiza permanece apenas na branch local de trabalho; antes de qualquer push é preciso reconstruir o histórico da branch para que o arquivo-fonte restrito não permaneça em commits antigos. A atribuição do trabalho de Luiza deve permanecer explícita. A substituição não está concluída e as cenas ainda não foram alteradas.
