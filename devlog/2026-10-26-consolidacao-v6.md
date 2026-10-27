# Consolidação V6

Dois backups completos dos arquivos não regeneráveis foram verificados por SHA-256, com .git original e bundle de todas as refs. A V6 foi incorporada em cópia do repositório original fora do OneDrive, preservando os históricos existentes sem reescrita.

As cinco cenas de build mantêm os mesmos bytes da V6 verificada. Materiais acadêmicos, montagens e relatórios históricos foram separados do jogo. Por instrução explícita, foram removidos 457 sprites sem uso atual e 116 dependentes antigos; 815 texturas Sprite permanecem, todas com prova de utilização. O inventário está em docs/SPRITES_EM_USO.csv. Os arquivos retirados são recuperáveis no pacote externo e nos backups.

Ferramentas de manutenção passaram a usar dados locais e saídas de relatório apropriadas. Dois testes foram atualizados para ler as cenas habilitadas no build, preservando suas asserções. A inspeção das cinco cenas encontrou zero scripts/prefabs faltantes e zero erros de shader. As rotas físicas passaram sem falhas (Labirinto 93/252 casts; Lua 167/451 casts) e a sonda de movimentação de Melantha/Poe passou nas 22 regiões, com narrativa suspensa apenas para teste.

Os resultados finais de EditMode/PlayMode, hashes dos commits, confirmação de push e atualização do checkout original ficam no relatório externo de consolidação. Não houve reconstrução artística, novo puzzle, história ou cutscene.
