# Memento Mori V3 — handoff visual para 18/09/2026

Este documento resume exatamente o que falta para concluir o prompt principal. A base funcional deve permanecer intacta.

Atualização de 22/09: as afirmações históricas abaixo sobre fontes não importadas estão superadas. Há 77 sprites importados e integração parcial nos quatro ambientes. O estado atual, testes novos e pendências estão em `docs/VISUAL_R2_PROGRESS_2026-09-22.md`.

## Ordem de execução definida

### Correção de escopo mais recente do usuário

Manter os pisos e paredes existentes de Quarto, Labirinto e Domínio da Lua. Q1/L1/D1 saem da fila de regeneração; não aplicar o novo atlas Q1 produzido nesta retomada. Esta decisão prevalece sobre as recomendações antigas de regenerar tiles neste documento. Continuar com móveis, detalhes, objetos interativos e estados, personagens e animações. Janela e cama R2 já geradas em ArtStaging/VisualRebuildR2, ainda sem importação. Geração voltou a funcionar nesta retomada; disponibilidade da Unity ainda não foi verificada.

1. Gerar o pacote visual completo: mapas, sprites de detalhe, personagens, portraits, UI e efeitos.
2. Gerar também todos os assets de interação e suas variações de estado: portas, falsa porta, selos, espelhos, fragmentos, poço, altar, portal, objetos do Quarto e elementos do ritual.
3. Importar cada asset isoladamente e conferir PPU, pivot, filtro, compressão, footprint, slicing e estados visuais.
4. Corrigir proporções e composição em Quarto, Labirinto, Domínio da Lua e FinalBeta.
5. Conferir colliders, câmera, sorting, animações, pontos de interação e transições de estado.
6. Executar playtest humano e CT funcional completo, verificando cada estado antes e depois da interação.
7. Rodar EditMode, PlayMode, testes de contrato e gerar um novo build Windows.

A etapa de validação só vale como conclusão depois da integração visual. A validação funcional existente é uma linha de base anterior à reconstrução visual.

## Assets interativos e estados obrigatórios

- Quarto: cama, janela, livros, pistas, objeto do ritual, saída e indicação de objetivo.
- Labirinto: portas, falsa porta, correntes, poço, obelisco, selos e bloqueios.
- Domínio da Lua: espelhos, estados dos reflexos, fragmentos, galeria dos ciclos, sigilo, portal e efeitos de ativação.
- Cada interação deve preservar o estado de gameplay existente e trocar apenas a representação visual correspondente: inativo, destacado, ativado, bloqueado, resolvido ou concluído.
- Nenhum sprite interativo será considerado pronto sem verificar o collider, o prompt, a mudança de estado e a persistência após transição de cena.

## Já realizado

- Inventário físico de 449 PNGs e manifesto por asset/cena/PPU/pivô.
- Fontes Cinzel, Cormorant Garamond e Unifraktur Maguntia importadas com glifos portugueses verificados.
- Logo U1 gerado e aplicado no MainMenu.
- Retratos C1 refeitos em pixel art e ligados ao DialogueManager.
- Folhas M1/P1 de locomoção geradas; M2/P2 de ações geradas; A1/A2 de Andrealphus geradas.
- Folhas U2/U3 geradas e fatiadas.
- Q1, L1, D1, D2 e D3 foram geradas inicialmente, mas ficaram com proporções inadequadas e devem ser substituídas.
- Pisos e paredes iniciais foram aplicados ao Labirinto e ao Domínio, mantendo Tilemaps e colisões.
- EditMode visual: 25/25.
- PlayMode visual: 12/12.
- CT Editor: 11/11.
- A validação funcional anterior continua preservada; não confundir com aprovação visual final.

## Regenerar usando assets físicos como referência

Não usar as folhas geradas atuais como referência. Usar estes arquivos reais:

### Quarto

- `Assets/Art/Sprites/Quarto/01_Tiles/01_tile_chao_madeira.png`
- `Assets/Art/Sprites/Quarto/01_Tiles/02_segmento_parede_gotica.png`
- `Assets/Art/Sprites/Quarto/02_Arquitetura/01_janela_com_cortinas.png`
- `Assets/Art/Sprites/Quarto/03_Moveis/01_cama.png`
- `Assets/Art/Sprites/Quarto/03_Moveis/03_escrivaninha.png`
- `Assets/Art/Sprites/Quarto/05_Ritual/01_tapete_ritual_grande.png`

Regenerar Q1 e a janela/cama de Q2. A janela deve ter aproximadamente 96x64 px lógicos (3x2 tiles), estar integrada à parede superior e nunca parecer um prop no chão. A cama deve manter o footprint de 2,5x4,8 unidades e encostar na parede esquerda.

### Labirinto

- `Assets/Art/Sprites/Labirinto/01_Tiles_Base/Floors/01_floor_clean_01.png` — 32x32
- `Assets/Art/Sprites/Labirinto/01_Tiles_Base/Walls/13_wall_top.png` — 32x32
- `Assets/Art/Sprites/Labirinto/02_Arquitetura/31_archway_top.png`
- `Assets/Art/Sprites/Labirinto/06_Gameplay_States/01_false_door_closed.png` — 64x96

Regenerar L1. Auditar L2/L3 e regenerar somente as peças cujo footprint não respeitar 32 PPU. L4 selos, L5 portal/VFX e props pequenos podem ser reutilizados após a auditoria.

### Domínio da Lua

- `Assets/Art/Sprites/DominioDaLua/01_Tiles_Base/01_floor_moon_clean_01.png` — 32x32
- `Assets/Art/Sprites/DominioDaLua/01_Tiles_Base/07_wall_moon_top.png` — 32x32
- `Assets/Art/Sprites/DominioDaLua/02_Arquitetura/14_arch_moon_tall.png` — 32x64
- `Assets/Art/Sprites/DominioDaLua/08_Puzzle_Espelhos/01_mirror_frame_base.png` — 64x96
- `Assets/Art/Sprites/DominioDaLua/05_Props_Interativos/43_crystal_cluster_small.png`

Regenerar D1, D2 e D3. D4 Black Mirror, D5 Galeria, D6 Sigilo/Fragmento e D7 portal/VFX devem ser auditados e só regenerados se estiverem fora desses footprints.

## Personagens e animações

- Reimportar M1/P1/A1 com PPU, pivô nos pés, filtro Point, mipmaps desligados e compressão None.
- Corrigir `melantha_walk_front/back/side.anim`: quatro frames únicos, 10 Hz, loop de 0,40 s, sem repetir o primeiro frame no fim.
- Conferir baseline dos pés de todos os quadros de Melantha; a caminhada atual está travada por repetição e desalinhamento.
- Criar/ligar estados de M2/P2/A2 sem alterar parâmetros dos controllers existentes.
- Validar idle, walk, follow, hint, observe, sit, vanish/reappear, float, talk, gesture, appear e disappear.
- Manter os retratos C1 em pixel art; não substituir por ilustração realista.

## Integração de cenas

- Aplicar Quarto final e repetir a composição em FinalBeta.
- Reposicionar janela, cama, escrivaninha, ritual e props com base nos footprints medidos.
- Verificar que corredores do Labirinto têm pelo menos 3 unidades e que as sete áreas continuam legíveis.
- Verificar jardim, espelhos, galeria, sigilo, fragmento e portal na escala do Domínio.
- Recalcular colliders apenas depois de escolher os sprites finais.
- Validar câmera URP, Pixel Perfect 640x360, PPU32, sorting por ponto de contato e Canvas Overlay.

## Validação final obrigatória

- Capturas visuais de MainMenu, Quarto, Labirinto, Domínio, FinalBeta, diálogo, pause e end card.
- Compilação sem erros.
- EditMode, PlayMode e CT Editor novamente após a integração.
- Build Windows em pasta nova, sem sobrescrever a build funcional anterior.
- CT da build Windows e conferência de log.
- Atualizar `FINAL_ART_PRODUCTION_MANIFEST.md`, `FINAL_ART_GENERATION_QUEUE.json`, inventário, devlog e este handoff.
- Só então preencher o relatório final do prompt com `PRONTO PARA REVISÃO DA CALLISTO: SIM`.

## Bloqueio atual

A geração e o Unity MCP responderam nesta retomada (Unity: projeto correto, porta 7890, MainMenu, fora de Play Mode). O bloqueio anterior de cota não está ativo nas chamadas realizadas.

Novas fontes e catálogo estão em `ArtStaging/VisualRebuildR2/`. Ainda não importadas: completar revisão de transparência/halos, alinhamento dos estados e alternância dos passos de frente/costas de Melantha. Ver `README.md` e `import-catalog.json` dessa pasta para distinguir fontes geradas, candidatos rejeitados e pendências. Não apresentar essas fontes como sprites já integrados ou puzzles revalidados.
