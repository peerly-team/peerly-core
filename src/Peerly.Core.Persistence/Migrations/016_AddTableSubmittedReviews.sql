-- +goose Up
-- +goose StatementBegin
create table submitted_reviews
(
    id                    bigserial primary key not null,
    submitted_homework_id bigint               not null,
    student_id            bigint               not null,
    mark                  int                  not null,
    comment               text                 not null,
    creation_time         timestamptz          not null
);
-- +goose StatementEnd

-- +goose Down
-- +goose StatementBegin
drop table submitted_reviews;
-- +goose StatementEnd
