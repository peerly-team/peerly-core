-- +goose Up
-- +goose StatementBegin
alter table teachers alter column name set not null;
alter table students alter column name set not null;
-- +goose StatementEnd

-- +goose Down
-- +goose StatementBegin
alter table students alter column name drop not null;
alter table teachers alter column name drop not null;
-- +goose StatementEnd
