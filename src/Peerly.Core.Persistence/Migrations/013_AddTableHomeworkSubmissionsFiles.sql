-- +goose Up
-- +goose StatementBegin
create table homework_submission_files
(
    homework_submission_id bigint not null,
    file_id                bigint not null,

    primary key (homework_submission_id, file_id)
);
-- +goose StatementEnd

-- +goose Down
-- +goose StatementBegin
drop table homework_submission_files;
-- +goose StatementEnd
