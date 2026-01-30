-- +goose Up
-- +goose StatementBegin
create table homework_files
(
    id            bigserial primary key not null,
    homework_id   bigint                not null,
    file_id       bigint                not null,
    creation_time timestamptz           not null
);
-- +goose StatementEnd

-- +goose Down
-- +goose StatementBegin
drop table homework_files;
-- +goose StatementEnd
