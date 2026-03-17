-- +goose Up
-- +goose StatementBegin
create table distribution_reviewers
(
    homework_id bigint not null,
    student_id  bigint not null,

    primary key (homework_id, student_id)
);
-- +goose StatementEnd

-- +goose Down
-- +goose StatementBegin
drop table distribution_reviewers;
-- +goose StatementEnd
