# Memento Mori — UI Beta

Pacote organizado de UI visual do beta.

## Estrutura
- `MainMenu/` — IDs 01–10
- `Gameplay/` — IDs 11–20
- `Screens/` — IDs 21–30 (Pause, Créditos e FinalBeta)

## Uso na Unity
- Importar como `Sprite (2D and UI)`.
- Preservar transparência alfa.
- Para pixel art: `Filter Mode = Point (no filter)` e `Compression = None`.
- Os textos (`JOGAR`, `CRÉDITOS`, `SAIR`, `CONTINUAR`, objetivos e diálogos) devem ser feitos com TextMeshPro.
- O fundo ilustrado do Main Menu **não está incluído**, pois será adicionado depois.
- Os botões 02–04 são os estados Normal / Hover / Pressed.
- O asset 10 é somente um overlay de glow transparente.
- Os assets 29–30 são bordas modulares reutilizáveis.

## Observação
Alguns ornamentos são propositalmente reutilizáveis em mais de uma tela para manter a identidade visual consistente do Memento Mori inteiro, sem limitar a UI ao Domínio da Lua.
