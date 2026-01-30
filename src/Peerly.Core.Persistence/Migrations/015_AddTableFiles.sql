-- +goose Up
-- +goose StatementBegin
create table files
(
    id            bigserial primary key not null,
    name          text                  not null,
    extension     text                  not null,
    creation_time timestamptz           not null
);
-- +goose StatementEnd

-- +goose Down
-- +goose StatementBegin
drop table files;
-- +goose StatementEnd
