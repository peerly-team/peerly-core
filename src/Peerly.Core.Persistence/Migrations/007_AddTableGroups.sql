-- +goose Up
-- +goose StatementBegin
create table groups
(
    id            bigserial primary key not null,
    course_id     bigint                not null,
    name          text                  not null,
    creation_time timestamptz           not null,
    update_time   timestamptz
);
-- +goose StatementEnd

-- +goose Down
-- +goose StatementBegin
drop table groups;
-- +goose StatementEnd
