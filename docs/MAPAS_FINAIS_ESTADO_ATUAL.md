# Mapas finais — estado da integração (03/10/2026)

Branch: `integration/mapas-finais`, isolada da `master`. A cena de referência da Luiza foi conservada em `Assets/mapa luiza/SampleScene.unity`. Suas posições de jardim guiaram a expansão da cena oficial, sem usar a imagem de referência como cenário.

## Reutilizar

- Tilemaps funcionais `V3MapArt/Floor`, `Walls` e `Collision`, câmera, player, gatilhos, portais, PuzzleMirror, EchoCorridorPuzzle, SigilRingPuzzle e sistema existente do Grimório.
- Tiles de piso e paredes R3/V3; visual e lógica existentes do Jardim, Espelhos, Corredor, Sigilo e Fragmento.
- Geometria do jardim na cena da Luiza: 573 posições de piso adaptadas à extensão leste da Lua.
- As 62 definições de tiles da Luiza usadas no jardim, mantendo os GUIDs e trocando apenas a arte dependente do pacote restrito.

## Refazer

- O piso do Labirinto abaixo do núcleo: rede conectada com rotas horizontais, laços, desvios, seis salas bloqueadas e Lua acessível. O trecho superior funcional permaneceu.
- Paredes e colisão geradas para a nova circulação, com salas cenográficas e molduras de colisão independentes.
- Espelhos: exigem Delayed → Ahead → Absent; erro apaga a tentativa parcial local.
- Corredor Ilusório da Lua: quatro espelhos ativos, sequência 2 → 1 → 3, erro com retorno local e progresso independente dos Ecos do Labirinto.
- Galeria: Crescente → Cheia → Minguante; exige o Corredor Ilusório, erro reinicia só a galeria e sucesso libera a parede da passagem.
- O arquivo `docs/INTEGRACAO_MAPAS_FINAIS_AUDITORIA.md` é uma auditoria inicial e descreve o estado anterior às alterações; este documento registra o estado atual.

## Gerar novo

- `L2_SealedDomainRooms.png`: seis composições de 512 × 512 para Diabo, Torre, Enforcado, Eremita, Morte e Julgamento, com objetos de portal e colisores separados.
- `L2_MoonActiveRoom.png`: sala Lua ativa para o Labirinto.
- `D2_LuizaGardenReplacement.png`: 64 sprites de uma unidade para substituir o pacote Gentle Forest, aplicados às 62 definições de tile.
- `GalleryCycleChoice.cs` e `LockedDomainPortal.cs`: interações pequenas que reutilizam os sistemas de estado e de interação.

## Substituir no mapa

- O Labirinto usa a folha L2 somente nas salas; as passagens continuam pintadas em Tilemaps. Os seis portais bloqueados receberam resposta própria, sem levar à cena da Lua. O portal da Lua continua funcional.
- A cena da Lua recebeu 777 células de decoração segura da composição da Luiza em seis camadas e um corredor de ligação. Pétalas, reflexo, arte do Jardim e checkpoint foram movidos com a região. A câmera foi estendida para x=53.
- A solução do Sigilo Fragmentado continua **Minguante → Grimório → SUSTENTAR**. `SigilRingPuzzle` existente foi preservado e exige a Galeria concluída na cena da Lua. O Grimório já desbloqueia entradas durante a exploração; a Galeria também registra pistas ao concluir o ciclo.

## Verificações realizadas e limites

- Labirinto: 4.461/4.461 células de piso conectadas; 151 colisores, 33 gatilhos, seis portais bloqueados e zero scripts ausentes.
- Lua: 3.971/3.975 células de piso conectadas na busca de quatro direções; todas as três pétalas movidas, o checkpoint do Jardim e o portal final estão no componente principal. Quatro células isoladas ainda precisam de análise visual. Há 129 colisores, 51 gatilhos, 5.367 tiles com sprite válido e zero scripts ausentes.
- O Unity compilou sem erros de script após as alterações. Testes automatizados: EditMode 25/25 e PlayMode 14/14 passaram (XMLs na pasta de evidências). O percurso automatizado MainMenu → Quarto → Labirinto → Lua → FinalBeta passou em 12/12 verificações, inclusive Corredor Ilusório, Poe, Jardim, Espelhos, Galeria e Sigilo. A inspeção visual manual final continua recomendada antes de publicação do jogo.
- A substituição de Gentle Forest está aplicada e seu blob foi excluído do histórico desta branch. Outros pacotes da entrega da Luiza ainda precisam de auditoria de licença individual; não há conclusão de redistribuição para eles neste relatório. A branch remota da Luiza conserva o pacote antigo e não foi modificada.

## Evidências locais

- `C:/Users/Usuario/Documents/MementoMori_Evidencias/Integracao_Mapas_Finais/Labirinto_Expanded_Render.png`
- `C:/Users/Usuario/Documents/MementoMori_Evidencias/Integracao_Mapas_Finais/MoonGarden_Expanded_Render.png`
- `docs/GENTLE_FOREST_REPLACEMENT_MANIFEST.csv` registra os 62 tiles substituídos.


- `C:/Users/Usuario/Documents/MementoMori_Evidencias/Integracao_Mapas_Finais/MoonIllusoryCorridor_Render_Final.png`
- `C:/Users/Usuario/Documents/MementoMori_Evidencias/Integracao_Mapas_Finais/EditModeFinalCorridor.xml` (25/25) e `PlayModeFinalCorridor.xml` (14/14)
- `TestResults/ct-evidence-current.json` (12/12)
