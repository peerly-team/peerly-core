-- +goose Up
-- +goose StatementBegin
create table rubrics
(
    id            bigserial primary key not null,
    teacher_id    bigint                not null,
    name          text                  not null,
    creation_time timestamptz           not null,
    update_time   timestamptz
);
-- +goose StatementEnd

-- +goose Down
-- +goose StatementBegin
drop table rubrics;
-- +goose StatementEnd
