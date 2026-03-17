# Скрипт для локальных миграций
CONNECTION_STRING='host=localhost port=54321 sslmode=disable dbname=peerly-core user=peerly-core-user password=pwd'
HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

goose -dir "$HERE" postgres "$CONNECTION_STRING" up

read -rp "Press any key to continue"
