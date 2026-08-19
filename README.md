# Memento Mori

Memento Mori é um jogo 2D narrativo de exploração e puzzles. A jornada acompanha Melantha por ambientes de fantasia gótica, com foco em diálogo, interação e descoberta.

## Fluxo jogável

`MainMenu → Quarto → Labirinto → DominioLua → FinalBeta`

- **MainMenu:** entrada e início de uma nova sessão.
- **Quarto:** apresentação, interações iniciais e ritual.
- **Labirinto:** travessia com portas, ecos, Poe e corredor ilusório.
- **DominioLua:** espelho, sigilo, fragmento e portal final.
- **FinalBeta:** encerramento da fatia vertical e retorno ao menu.

## Estrutura do projeto

- `Assets/` — código, cenas, prefabs, áudio, testes e recursos usados pelo jogo.
- `Assets/Scripts/` — scripts C# responsáveis pelos sistemas de jogo.
- `Assets/Scenes/` — cenas do fluxo principal.
- `Assets/LuizaAssets/` — materiais visuais usados na reconstrução das fases. Alguns packs ficam apenas no ambiente local por restrições de licença.
- `Packages/` — dependências do projeto Unity.
- `ProjectSettings/` — configurações do projeto Unity.
- `docs/` — arquitetura, roadmap, créditos, licenças e instruções de instalação dos assets locais.

Dentro de `Assets/`, arquivos `.cs` são códigos C#; `.unity` são cenas; `.prefab` são objetos reutilizáveis; `.asset` guarda configurações e dados do Unity; `.anim` e `.controller` tratam animações; `.asmdef` define assemblies; e `.meta` preserva os identificadores usados pelo Unity. Não remova ou mova arquivos `.meta` isoladamente.

## Como abrir

1. Instale o Unity `6000.4.12f1`.
2. Abra esta pasta pelo Unity Hub.
3. Aguarde a importação dos assets.
4. Abra `Assets/Scenes/MainMenu.unity` e use Play.

## Estado técnico

Na validação mais recente, o projeto compilou e as suítes EditMode, PlayMode e smoke passaram. Esses resultados cobrem os contratos técnicos; a aprovação visual e o playthrough humano continuam sendo etapas próprias.

## Equipe

- **Callisto:** programação, gameplay, integração, puzzles, áudio técnico, testes e build.
- **Luiza:** direção visual, tilemaps, sprites, props, composição das cenas, colliders de cenário e UI visual.
- **Ambas:** testes, Git, documentação e apresentação.

## Próximos passos

- Reconstruir a camada visual das cenas.
- Integrar sprites, tilemaps, colliders, sorting e UI.
- Fazer playthrough humano, QA visual, build e entrega acadêmica.

## Git

Use branches para mudanças isoladas e abra uma revisão antes de integrar alterações na principal. Arquivos gerados pelo Unity e packs locais restritos já são ignorados pelo Git.

## Créditos e licenças

Consulte [Assets e licenças](docs/ASSETS_AND_LICENSES.md). Para instalar os packs que não são versionados, siga [Configuração de assets](docs/ASSET_SETUP.md).
