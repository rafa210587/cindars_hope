# Continuar no notebook e voltar ao desktop

Preparado em 19/10/2026 para o Cindar's Hope. Execute os comandos em **PowerShell 7 externo**, com Unity e Codex fechados. O pacote é uma pasta transportável; pode ficar em SSD externo ou ser copiado pela rede. Nenhum script faz push, commit, merge ou apaga o projeto original.

## 1. O que precisa ir

O exportador leva a pasta real do projeto, incluindo `.git` (branches, commits locais, index e objetos LFS), arquivos não commitados, não rastreados, `.specs`, harness, `memory/`, `art/`, fontes Aseprite e evidências locais. Não depende de esses arquivos estarem no Git. Só exclui `Library`, `Temp`, `Obj`, `Logs` e `.vs` **na raiz**; o Unity recria esses caches. Builds, UserSettings e fontes locais são preservados.

Também leva o estado local de `%USERPROFILE%\.codex` ou `CODEX_HOME`: sessões, sessões arquivadas, índices, bancos SQLite com arquivos WAL, memórias, skills, plugins, imagens, anexos, visualizações e worktrees presentes nessa pasta. Copia `.agents` global, se existir. Há caches de plugins e outros worktrees que podem deixar o pacote grande. Pastas de navegador, sandbox, alguns caches temporários e credenciais conhecidas (`auth.json`, `cap_sid`) ficam fora. As configurações e os históricos podem conter informações privadas; mantenha o pacote em armazenamento pessoal protegido.

Saves no caminho Unity padrão, obtido de companyName/productName do projeto, e outros worktrees existentes registrados no Git são arquivados automaticamente em `extras/` quando estiverem fora das pastas já copiadas. Outras pastas externas entram por `-ExtraPath`. **Extras não são restaurados automaticamente**: `transfer.json` registra origem e pasta correspondente.

Programas instalados, drivers, licenças, credenciais do Windows, conexões OAuth, chaves SSH, variáveis de ambiente e serviços externos não são instalados pelos scripts. Memória do servidor MCP externo só viaja se sua pasta/banco for incluída; `memory/` e `.ai-memory.toml` sozinhos não substituem esse banco.

## 2. Preparar o notebook

Instale Codex, Git para Windows, PowerShell 7 e Unity Hub. No Hub, instale **Unity 6000.5.7f1**, versão registrada em `ProjectSettings/ProjectVersion.txt`, com os módulos de plataforma usados no projeto. Instale Git LFS se for usá-lo; instale .NET/IDE e Python/uv/Node conforme os runners e MCPs necessários. Aseprite exige sua instalação/licença. ComfyUI, modelos e ferramentas externas precisam de cópia ou instalação próprias.

Abra o Codex uma vez para concluir a instalação e depois feche-o completamente, inclusive processos em segundo plano. Use versão compatível com a origem, preferencialmente a mesma ou mais recente. Evite criar trabalho no Codex do notebook antes da restauração: o script preserva a pasta anterior, mas não mescla históricos SQLite.

Prefira manter o caminho `D:\Projetos\Cindars_Hope\cindars_hope` e, se possível, o mesmo nome de usuário Windows. Se o notebook não tiver `D:`, use outro caminho e revise referências absolutas depois. O script não cria unidades nem modifica bancos para simular caminhos antigos.

## 3. Exportar no desktop

Salve as cenas/assets no Unity e conclua as tarefas em execução. Feche Unity e Codex. Não execute o exportador por dentro de um chat ativo: ele recusa processos abertos para evitar copiar SQLite ou arquivos em alteração.

Em PowerShell externo:

```powershell
cd D:\Projetos\Cindars_Hope\cindars_hope
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\codex\Export-NotebookTransfer.ps1 -Destination E:\Transferencia-Cindars-ida
```

Troque `E:` pela unidade do SSD/pendrive. O destino precisa ser uma **pasta nova, fora do projeto e do CODEX_HOME**. O original permanece intacto. O pacote inclui o importador, sua biblioteca e este guia na raiz.

Para incluir outra ferramenta externa, use uma chamada PowerShell com array (somente se a pasta existir):

```powershell
& .\tools\codex\Export-NotebookTransfer.ps1 `
  -Destination E:\Transferencia-Cindars-ida `
  -ExtraPath @('D:\Projetos\Cindars_Hope\.serena-runtime')
```

O checkout `cindars_hope_ci_pilot_index_20260912` estava registrado na inspeção de 19/10 e entra automaticamente como extra. Outros caminhos úteis, se houver trabalho neles: `.claude` pessoal, projetos fora desta pasta, banco de ai-memory, arquivos em Downloads, instalações/fontes de ferramentas próprias e modelos ComfyUI. Ambientes Python/venvs podem exigir recriação, mesmo copiados. O `node_repl` global desta máquina aponta para um runtime dentro de `AppData\Local\OpenAI\Codex\runtimes`; deixe o app provisionar o runtime no notebook e revise a configuração em vez de assumir o caminho antigo.

Revise `transfer.json`: branch, HEAD, estado Git completo, worktrees registrados, extras e exclusões. A pasta só está pronta quando o comando termina com `Integridade: ... PASS` e `Pacote pronto`. Se falhar, preserve a pasta incompleta para diagnóstico e tente em outro destino novo. Não desligue o desktop antes de validar a cópia no notebook.

## 4. Restaurar no notebook

Copie a pasta inteira do pacote. Com Unity/Codex fechados, valide primeiro:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File E:\Transferencia-Cindars-ida\Import-NotebookTransfer.ps1 -PackageRoot E:\Transferencia-Cindars-ida -VerifyOnly
```

Restaure projeto e estado Codex:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File E:\Transferencia-Cindars-ida\Import-NotebookTransfer.ps1 -PackageRoot E:\Transferencia-Cindars-ida -RestoreCodex
```

Sem `-ProjectDestination`, o importador usa o caminho original registrado. Para mudar:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File E:\Transferencia-Cindars-ida\Import-NotebookTransfer.ps1 -PackageRoot E:\Transferencia-Cindars-ida -ProjectDestination C:\Projetos\cindars_hope -RestoreCodex
```

O destino do projeto **não pode existir**. Se já existir, use outra pasta para comparar depois. A pasta Codex existente é renomeada para `.codex.antes-transferencia-<data>` ao lado da original; por segurança, precisa ser pasta comum com `config.toml`. Se não tiver esse arquivo, preserve/renomeie manualmente a pasta antes de importar. Não há merge de bancos nem sobrescrita do projeto existente. Se houver `.agents` global nas duas máquinas, o importador interrompe antes de escrever: compare/restaure essas skills manualmente. Sem `-RestoreCodex`, ele restaura só o projeto.

O importador verifica SHA-256 do pacote e dos arquivos do projeto restaurado, HEAD e estado Git. Em caso de falha, não abra o Unity até entender a diferença; o pacote e a pasta anterior continuam disponíveis. Para desfazer o estado Codex, feche o app, preserve a pasta importada com outro nome e devolva o nome original à pasta `antes-transferencia`; não una os bancos.

Restaure manualmente os extras que precisar, consultando `transfer.json`. Para saves, copie a pasta indicada para o persistentDataPath do notebook antes de iniciar o jogo e preserve qualquer save já existente. Para worktrees externos, restaure os checkouts no caminho desejado e use `git worktree repair <caminho>` a partir do repositório principal. Isso ajusta metadados locais; não use prune/reset para tentar recuperar trabalho. Se mantiver caminhos diferentes, os worktrees em `.codex/worktrees` também podem precisar desse reparo. Links/junctions não são seguidos: links copiados interrompem a verificação, e junctions excluídas precisam ter seu alvo transferido separadamente.

## 5. Retomar o trabalho

Antes de abrir o Codex, revise `%USERPROFILE%\.codex\config.toml`, `.codex/config.toml` do projeto e hooks/skills com caminhos absolutos, especialmente `C:\Users\Rafa` e `D:\Projetos`. Confirme Node/uvx e permissões locais. Serena permanece opcional/desabilitada conforme a configuração atual. Bancos SQLite, JSONL e TOML não recebem substituições automáticas de caminhos. Automações TOML importadas com status ACTIVE são colocadas em PAUSED; outros agendamentos eventualmente registrados nos bancos exigem conferência no app. Agendadores externos ao Codex não são transferidos.

Abra o Codex, entre na mesma conta, adicione a pasta restaurada e reconecte plugins e MCP Unity. Confira os agendamentos antes de retomá-los e mantenha a origem fechada. Os arquivos de histórico foram copiados; **a exibição e retomada exata dos chats depende da compatibilidade da versão do app**. Esta transferência local não é uma garantia oficial de sincronização de históricos entre máquinas. Caso os chats não apareçam, preserve os bancos e abra um chat novo com a mensagem abaixo.

Abra o projeto no Unity Hub com a versão correta, aguarde a reconstrução de Library e confira a compilação. Regenerar Library leva tempo e exige espaço; referências locais de Packages também precisam existir. Não atualize o Unity durante a migração. Faça validação proporcional à tarefa retomada; os resultados antigos continuam sendo evidência histórica da máquina original.

Mensagem para o primeiro chat no notebook:

```text
Estou continuando o Cindar's Hope de outro computador durante uma semana.
Leia AGENTS.md/CLAUDE.md e docs/project/CURRENT_STATE.md. O projeto veio por copia
integral, com .git e trabalho nao commitado. Preserve todos os edits existentes.
Use tools/codex/Get-ChangeSummary.ps1 e confira o inventario transfer.json do pacote.
Nao assuma que CURRENT_STATE descreve todas as mudancas recentes; confronte a tarefa
que eu indicar com os arquivos atuais. Nao reexecute specs encerradas nem declare
aceite humano com base em testes automatizados. Nao faca push, PR, merge ou limpeza.
Primeiro confira caminhos/dependencias e reporte bloqueios reais. Vou indicar a tarefa
a retomar. Responda em portugues, curto e direto.
```

Na inspeção inicial havia **434 paths alterados/novos**, branch `dev`, HEAD `1a905e54f4d4ad83d28b60a5a466bd983448f02d`; estes números mudam com o trabalho. O manifesto gerado na exportação é a fotografia efetiva. `CURRENT_STATE.md` registra limitações e validações de setembro; esta migração não promove specs nem comprova aceitação visual/humana.

## 6. Voltar ao desktop após a troca da CPU

Durante a semana, trate o notebook como a cópia principal. Antes de voltar, salve tudo, feche os apps e execute **o mesmo exportador no notebook**, em um destino novo como `E:\Transferencia-Cindars-volta`. O pacote de ida continua guardado.

No desktop, preserve a pasta antiga e importe o pacote de volta para uma pasta nova, por exemplo `D:\Projetos\Cindars_Hope\cindars_hope_retorno`, com `-RestoreCodex`. Revise caminhos, restaure os extras/saves atuais e valide. Se quiser recuperar o caminho original, feche os apps e renomeie as pastas preservando ambas; repare os worktrees depois. Não sobreponha o desktop antigo ao notebook atualizado. Se trabalhou nos dois computadores, compare/reconcilie antes de eleger a cópia principal.

## Referências e validação dos scripts

As camadas de configuração pessoal e do projeto estão documentadas em [Config basics, OpenAI](https://learn.chatgpt.com/docs/config-file/config-basic); a instalação Windows em [documentação oficial do aplicativo](https://learn.chatgpt.com/docs/windows/windows-app). A cópia dos bancos locais é uma estratégia de backup baseada nos arquivos observados nesta máquina, sem contrato oficial de portabilidade de SQLite.

Teste local com fixture isolada:

```powershell
pwsh -NoProfile -File tools/codex/Test-NotebookTransfer.ps1
```

Esse teste verifica preservação de trabalho Git, fontes ignoradas, exclusão de caches, credenciais conhecidas, integridade, restauração e recusas de destino/tampering. Não abre Unity/Codex nem verifica o notebook real. A exportação final exige fechar este chat/app e executar o comando externo.

Resultado local em 19/10/2026: **18 verificações PASS**, com 65 arquivos conferidos por hash na fixture, incluindo outro worktree registrado. Exportação do ambiente real e restauração no notebook ainda não executadas; Unity não foi validado nesta entrega de documentação/scripts.
