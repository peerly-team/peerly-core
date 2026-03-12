-- +goose Up
-- +goose StatementBegin
create table review_submissions
(
    id            bigint primary key not null,
    homework_id   bigint             not null,
    student_id    bigint             not null,
    mark          int                not null,
    comment       text               not null,
    creation_time timestamptz        not null
);
-- +goose StatementEnd

-- +goose Down
-- +goose StatementBegin
drop table review_submissions;
-- +goose StatementEnd
