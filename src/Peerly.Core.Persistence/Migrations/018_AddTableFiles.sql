-- +goose Up
-- +goose StatementBegin
create table files
(
    id            bigserial primary key not null,
    storage_id    uuid                  not null,
    name          text                  not null,
    size          int                   not null,
    creation_time timestamptz           not null
);
-- +goose StatementEnd

-- +goose Down
-- +goose StatementBegin
drop table files;
-- +goose StatementEnd
