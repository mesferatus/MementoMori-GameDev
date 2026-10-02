# Prompt operacional — fase de arte e mapas

Você trabalha no projeto Unity `MementoMori-GameDev`, um jogo 2D em Unity 6/URP, com arte pixel art em `Assets/Art`, cenas em `Assets/Scenes`, scripts em `Assets/Scripts` e testes EditMode/PlayMode.

## Fase atual

A prioridade é construir a base visual jogável:

- validar e organizar os assets existentes;
- montar os mapas e ambientes das cenas oficiais;
- configurar Tilemaps, Tilemap Renderer, colisões e limites físicos;
- deixar os espaços prontos para personagens e narrativa posteriores.

Personagens, diálogos completos, puzzles e progressão narrativa ainda não são o foco. Não invente personagens, sprites, mapas ou sistemas narrativos para preencher lacunas.

## Skills prioritárias

- `2d-pixel-perfect`: câmera, escala e preservação da pixel art;
- `sprite-editor`: importação, pivôs, filtros, compressão e transparência;
- `tilemap-palette-create`: preparação de paletas e pintura dos mapas;
- `tilemap-ruletile-createempty` ou `tilemap-ruletile-createfromsegment`: somente quando houver tiles suficientes e uma regra comprovada;
- `unity-cli`: Editor, validação, testes e build;
- `ui-ugui`: apenas para UI necessária ao mapa.

## Regras de execução

1. Inspecione o estado atual e o `git status` antes de alterar qualquer coisa. Preserve mudanças locais do usuário.
2. Consulte `README.md`, `CONTRIBUTING.md`, `docs/`, `Assets/Tests/` e os padrões existentes.
3. Audite os assets físicos em `Assets/Art` antes de criar qualquer coisa. Não crie PNG, arte substituta ou ZIP duplicado.
4. Preserve nomes, pastas, `.meta`, GUIDs e configurações válidas.
5. Monte uma cena por vez, nesta ordem: `Quarto`, `Labirinto`, `DominioLua`, `FinalBeta`, `MainMenu`.
6. Use Tilemaps para chão, paredes, limites e elementos repetitivos. Use GameObjects separados somente para objetos individuais.
7. Configure `TilemapCollider2D` nos Tilemaps de colisão. Use `CompositeCollider2D` somente quando necessário e validado.
8. Não assuma que colisores serão criados automaticamente em sprites decorativos ou objetos isolados. Verifique camadas, máscaras, `isTrigger`, Rigidbody2D, limites e spawn points.
9. Valide no Editor e em Play Mode se o jogador caminha, não atravessa paredes e não fica preso. Isso não equivale a validar o gameplay completo.
10. Não implemente personagens, IA, diálogos, puzzles ou progressão nesta fase, salvo pedido explícito.
11. Antes de editar cenas ou prefabs, verifique se há Editor conectado. Nunca edite YAML de `.unity` ou `.prefab` enquanto houver Editor vivo.
12. Faça mudanças incrementais e valide cada uma. Não reconstrua hierarquias inteiras sem necessidade.
13. Ao finalizar, informe arquivos alterados, testes executados, resultado visual/físico e pendências reais. Não invente playtest, integração, commit ou release.

## Fases posteriores

Quando a base visual estiver aprovada, as tarefas de personagens, narrativa, diálogos, puzzles e progressão deverão ser tratadas separadamente, preservando os mapas construídos e registrando dependências entre cena, objeto, script, asset e estado.

Não faça commit ou push sem autorização explícita.
